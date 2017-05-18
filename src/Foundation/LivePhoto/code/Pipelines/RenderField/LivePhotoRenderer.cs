using System.Text;
using Glass.Mapper;
using Paragon.Foundation.LivePhoto.Data.Fields;
using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Sites;
using Sitecore.Text;
using Sitecore.Xml.Xsl;
using Context = Sitecore.Context;

namespace Paragon.Foundation.LivePhoto.Pipelines.RenderField
{
    // Based on the 
    public class LivePhotoRenderer : FieldRendererBase
    {
        private LivePhotoField livePhotoDataField { get; set; }
        private Database database;
        private Language language;
        private Version version;
        private int height;
        private int width;
        private string mediaSrc;
        private string movieSrc;
        private string emptyMessage;
        private string tag;
        private string className;

        public Item Item { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public SafeDictionary<string> Parameters { get; set; }

        public virtual RenderFieldResult Render(string dataAttributes)
        {
            var site = Context.Site;
            var item = Item;
            var innerField = item?.Fields[FieldName];

            if (item == null || innerField == null)
                return RenderFieldResult.Empty;
            
            livePhotoDataField = new LivePhotoField(innerField, innerField.Value);
            ParseField(livePhotoDataField);
            ParseNode(Parameters);

            if ((string.IsNullOrEmpty(mediaSrc) || IsBroken(livePhotoDataField)) && site != null && site.DisplayMode == DisplayMode.Edit)
            {
                mediaSrc = GetDefaultImage();
                className += " scEmptyImage";
                className = className.TrimStart(' ');
                emptyMessage = Translate.Text("Field not Set");
            }

            if (string.IsNullOrEmpty(mediaSrc))
                return RenderFieldResult.Empty;
           
            var imgSource = GetPhotoSource();
            var movSource = GetMovieSource();
            var stringBuilder = new StringBuilder($"<{tag} {dataAttributes}");
            AddAttribute(stringBuilder, "data-photo-src", imgSource);
            AddAttribute(stringBuilder, "data-video-src", movSource);
            AddAttribute(stringBuilder, "style", $"height:{height}px; width:{width}px; background:#e4e4e4;");

            if (!string.IsNullOrEmpty(className))
                AddAttribute(stringBuilder, "class", className);

            stringBuilder.Append($">{emptyMessage}</{tag}>");
            return new RenderFieldResult(stringBuilder.ToString());
        }

        protected virtual string GetPhotoSource()
        {
            var options = new MediaUrlOptions();

            if (language != null)
                options.Language = language;

            options.Database = database;
            options.Version = version;
            options.MaxHeight = height;
            options.MaxWidth = width;

            var urlString = livePhotoDataField.PhotoMediaItem == null 
                ? new UrlString(mediaSrc) 
                : new UrlString(MediaManager.GetMediaUrl(livePhotoDataField.PhotoMediaItem, options));

            return urlString.GetUrl();
        }

        protected virtual string GetMovieSource()
        {
            var options = new MediaUrlOptions();

            if (language != null)
                options.Language = language;

            options.Database = database;
            options.Version = version;

            var urlString = livePhotoDataField.MovieMediaItem == null 
                ? new UrlString(movieSrc) 
                : new UrlString(MediaManager.GetMediaUrl(livePhotoDataField.MovieMediaItem, options));

            return urlString.GetUrl();
        }

        protected virtual void ParseNode(SafeDictionary<string> attributes)
        {
            tag = Extract(attributes, "tag");
            tag = string.IsNullOrWhiteSpace(tag) ? "div" : tag; 

            mediaSrc = Extract(attributes, "data-photo-src");
            movieSrc = Extract(attributes, "data-video-src");
            height = Extract(attributes, "height").ToInt();
            width = Extract(attributes, "width").ToInt();
            className = Extract(attributes, "class");
        }

        protected virtual void ParseField(LivePhotoField photofield)
        {
            database = photofield.MediaDatabase ?? Factory.GetDatabase("master");
            language = photofield.MediaLanguage;
            version = photofield.MediaVersion;

            if (photofield.PhotoMediaItem != null)
                mediaSrc = StringUtil.GetString(new string[2]
                {
                    mediaSrc,
                    photofield.PhotoMediaItem.Paths.FullPath
                });

            if (photofield.MovieMediaItem != null)
                movieSrc = StringUtil.GetString(new string[2]
                {
                    movieSrc,
                    photofield.MovieMediaItem.Paths.FullPath
                });
        }

        protected virtual string Extract(SafeDictionary<string> values, params string[] keys)
        {
            foreach (var key in keys)
            {
                var str = values[key];

                if (str == null)
                    continue;

                values.Remove(key);
                return str;
            }

            return string.Empty;
        }

        protected virtual bool IsBroken(LivePhotoField field)
        {
            return field?.PhotoMediaItem == null;
        }

        protected virtual string GetDefaultImage()
        {
            return Themes.MapTheme(Client.GetItemNotNull("/sitecore/content/Applications/WebEdit/WebEdit Texts", Client.CoreDatabase)["Default Image"]);
        }
    }
}