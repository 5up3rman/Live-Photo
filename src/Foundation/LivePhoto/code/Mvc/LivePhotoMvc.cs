using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.UI;
using Glass.Mapper;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Web.Mvc;
using Paragon.Foundation.LivePhoto.Extensions;
using Paragon.Foundation.LivePhoto.HtmlHelpers;
using Paragon.Foundation.LivePhoto.Models;
using Sitecore.Collections;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines;
using Sitecore.Pipelines.RenderField;
using Sitecore.Resources.Media;
using Sitecore.Text;
using Sitecore.Web;
using Utilities = Glass.Mapper.Utilities;

namespace Paragon.Foundation.LivePhoto.Mvc
{
    public class LivePhotoMvc<TK> : GlassHtmlMvc<TK>
    {
        protected new IGlassHtml GlassHtml { get; private set; }
        protected new TextWriter Output { get; private set; }

        public LivePhotoMvc(IGlassHtml glassHtml, TextWriter output, TK model) : base(glassHtml, output, model)
        {
            GlassHtml = glassHtml;
            Output = output;
            Model = model;
        }

        public HtmlString RenderLivePhoto<T>(T model, Expression<Func<T, object>> field, 
            bool isEditable = false, HtmlTextWriterTag tag = HtmlTextWriterTag.Span)
        {
            var fld = field.Compile().Invoke(model) as LivePhotoObject;

            if (fld == null)
                return new HtmlString("Field is not Set");

            var attr = GetLivePhotoCommonAttributes(fld, tag);
            var urlString = new UrlString();

            foreach (var keyValuePair in attr)
                urlString.Parameters.Add(keyValuePair.Key, keyValuePair.Value);

            if (!Glass.Mapper.Sc.GlassHtml.IsInEditingMode || !IsInEditingMode)
                return RenderLivePhoto(fld, tag);

            return new HtmlString(MakeEditable(field, model, urlString.Parameters, LivePhotoAPIDataAttributes(fld), SitecoreContext.GlassContext));
        }

        /// <summary>
        /// Render out the complete Tag
        /// </summary>
        /// <param name="fld"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private HtmlString RenderLivePhoto(ILivePhotoObject fld, HtmlTextWriterTag tag = HtmlTextWriterTag.Span)
        {
            var firstPart = $"<{tag.ToString().ToLower()} {string.Join(" ", LivePhotoAPIDataAttributes(fld))} {PropertyExtensions.ConvertAttributes(GetLivePhotoCommonAttributes(fld, tag), "")} style=\"height:{fld.Height}px; width:{fld.Width}px;\">";
            var lastPart = $"</{tag.ToString().ToLower()}>";

            return new HtmlString(firstPart + lastPart);
        }

        private string MakeEditable<T>(Expression<Func<T, object>> field, T target, object parameters, IEnumerable<string> attributes, Context context)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);
            MakeEditableLivePhoto(field, target, parameters, attributes, stringWriter, context).Dispose();
            stringWriter.Flush();
            stringWriter.Close();

            return sb.ToString();
        }

        /// <summary>
        /// Helps perform the Glass Editing Magic
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="model"></param>
        /// <param name="parameters"></param>
        /// <param name="dataAttributes"></param>
        /// <param name="writer"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private RenderElement MakeEditableLivePhoto<T>(Expression<Func<T, object>> field, T model, object parameters,
            IEnumerable<string> dataAttributes, TextWriter writer, Context context)
        {
            var firstPart = string.Empty;
            var lastPart = string.Empty;
            var attributes = new SafeDictionary<string>();
            
            try
            {
                if (model == null)
                    throw new NullReferenceException("No model set");

                if (parameters is string)
                    attributes = WebUtil.ParseQueryString(parameters as string);
                else if (parameters is NameValueCollection)
                {
                    var nameValueCollection = (NameValueCollection)parameters;

                    foreach (var key in nameValueCollection.AllKeys)
                        attributes.Add(key, nameValueCollection[key]);
                }
                else
                {
                    var propertiesCollection = PropertyExtensions.GetPropertiesCollection(parameters, true);

                    foreach (var key in propertiesCollection.AllKeys)
                        attributes.Add(key, propertiesCollection[key]);
                }

                // The following Utilities are using Glass Mapper.  I did not decompile and 'borrow' them this time :)
                MemberExpression memberExpression;
                var targetObjectOfLamba = Utilities.GetTargetObjectOfLamba<T>(field, model, out memberExpression);
                var typeConfiguration = Utilities.GetTypeConfig<T, SitecoreTypeConfiguration>(field, context, model);
                var glassProperty = Utilities.GetGlassProperty<T, SitecoreTypeConfiguration>(field, context, model);
                var item = typeConfiguration.ResolveItem(targetObjectOfLamba, SitecoreContext.Database);

                using (new ContextItemSwitcher(item))
                {
                    var fieldConfiguration = (SitecoreFieldConfiguration) glassProperty;
                    var renderFieldArgs = new RenderFieldArgs
                    {
                        Item = item,
                        FieldName = fieldConfiguration.FieldId == (ID) null || fieldConfiguration.FieldId == ID.Null
                                    ? fieldConfiguration.FieldName : fieldConfiguration.FieldId.ToString(),
                        Parameters = attributes,
                        Before = string.Join(" ", dataAttributes),
                        DisableWebEdit = false
                    };

                    // Running this will run the GetLivePhotoValue pipeline.
                    CorePipeline.Run("renderField", renderFieldArgs);
                    firstPart = renderFieldArgs.Result.FirstPart;
                    lastPart = renderFieldArgs.Result.LastPart;
                }
            }
            catch (Exception ex)
            {
                firstPart = string.Format("<p>{0}</p><pre>{1}</pre>", new object[] {ex.Message, ex.StackTrace});
                Sitecore.Diagnostics.Log.Error("Failed to render field", ex, typeof (IGlassHtml));
            }

            return new RenderElement(writer, firstPart, lastPart);
        }

        /// <summary>
        /// Set all the common, necessary data attributes 
        /// </summary>
        /// <param name="fld"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        private SafeDictionary<string> GetLivePhotoCommonAttributes(ILivePhotoObject fld, HtmlTextWriterTag tag = HtmlTextWriterTag.Span)
        {
            var collection = new SafeDictionary<string>();

            if (fld == null)
                return collection;

            PropertyExtensions.AddAttribute(collection, "data-photo-src", HttpUtility.HtmlEncode(GetProtectedPhotoSource(fld)));
            PropertyExtensions.AddAttribute(collection, "data-video-src", fld.DataVideoSrc);
            PropertyExtensions.AddAttribute(collection, "height", fld.Height.ToString());
            PropertyExtensions.AddAttribute(collection, "width", fld.Width.ToString());
            PropertyExtensions.AddAttribute(collection, "inlineStyle", fld.InlineStyle);
            PropertyExtensions.AddAttribute(collection, "tag", tag.ToString().ToLower());

            return collection;
        }

        private IEnumerable<string> LivePhotoAPIDataAttributes(ILivePhotoObject fld)
        {
            // Add the API Options. They need to added to the tag without a following ="", the API takes care of that.
            var dataAttrList = new List<string> { "data-live-photo" };

            if (!string.IsNullOrEmpty(fld.DataPhotoTime))
                dataAttrList.Add("data-photo-time");

            if (!string.IsNullOrEmpty(fld.DataShowsNativeControls))
                dataAttrList.Add("data-show-native-controls");

            return dataAttrList;
        }

        /// <summary>
        /// Sets the Dimensions and Media Protection Hash to the Image Url... or it's supposed to add the Hash
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private string GetProtectedPhotoSource(ILivePhotoObject fld)
        {
            var collection = new SafeDictionary<string>();

            if (!collection.ContainsKey("h"))
                collection.Add("h", fld.Height.ToString());

            if (!collection.ContainsKey("w"))
                collection.Add("w", fld.Width.ToString());

            var urlBuilder = new Glass.Mapper.UrlBuilder(fld.DataPhotoSrc);

            // Add the height and width 
            foreach (var key in collection.Keys)
                urlBuilder.AddToQueryString(key, collection[key]);

            return HttpUtility.HtmlEncode(ProtectMediaUrl(urlBuilder.ToString()));
        }

        private string ProtectMediaUrl(string url)
        {
            return HashingUtils.ProtectAssetUrl(url);
        }
    }
}