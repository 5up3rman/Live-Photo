using System;
using Paragon.Foundation.LivePhoto.Fields;
using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace Paragon.Foundation.LivePhoto.Commands.WebEdit
{
    [Serializable]
    public class ChooseLivePhotoMovie : WebEditImageCommand
    {
        public override void Execute(CommandContext context)
        {
            ExplodeParameters(context);
            var formValue = WebUtil.GetFormValue("scPlainValue");
            context.Parameters.Add("fieldValue", formValue);

            Context.ClientPage.Start(this, "Run", context.Parameters);
        }

        protected static void Run(ClientPipelineArgs args)
        {
            var itemNotNull = Client.GetItemNotNull(args.Parameters["itemid"], Language.Parse(args.Parameters["language"]));
            itemNotNull.Fields.ReadAll();

            var innerField = itemNotNull.Fields[args.Parameters["fieldid"]];
            var livePhotoField = new LivePhotoField(innerField, innerField.Value);
            var controlId = args.Parameters["controlid"];
            var fieldVal = args.Parameters["fieldValue"];

            if (args.IsPostBack)
            {
                if (args.Result == "undefined")
                    return;

                var itmVal = string.Empty;

                if (!string.IsNullOrEmpty(args.Result))
                {
                    var movieItem = (MediaItem) Client.ContentDatabase.GetItem(args.Result);

                    if (movieItem == null || !movieItem.Extension.Contains("mov"))
                        throw new ClientAlertException("Please select an Item that's a .mov type.");

                    livePhotoField.SetAttribute("movieid", movieItem.ID.ToString());

                    itmVal = livePhotoField.Value;
                }
                else
                    itmVal = string.Empty;

                SheerResponse.SetAttribute("scHtmlValue", "value", RenderImage(args, itmVal));
                SheerResponse.SetAttribute("scPlainValue", "value", itmVal);
                SheerResponse.Eval($"scSetHtmlValue('{controlId}')");
            }
            else
            {
                var itmVal = StringUtil.GetString(new string[2] { innerField.Source, "/sitecore/media library" });

                if (fieldVal.Length > 0)
                    fieldVal = new XmlValue(fieldVal, "image").GetAttribute("movieid");

                var movieId = fieldVal;

                if (itmVal.StartsWith("~", StringComparison.InvariantCulture))
                {
                    if (string.IsNullOrEmpty(movieId))
                        movieId = StringUtil.Mid(itmVal, 1);

                    itmVal = "/sitecore/media library";
                }

                var language = itemNotNull.Language;
                var mediaBrowserOptions = new MediaBrowserOptions();
                var image = Client.ContentDatabase.GetItem(itmVal, language);

                if (image == null)
                    throw new ClientAlertException("The source of this movie field points to an item that does not exist.");

                mediaBrowserOptions.Root = image;
                mediaBrowserOptions.AllowEmpty = true;

                if (!string.IsNullOrEmpty(movieId))
                {
                    var movItm = Client.ContentDatabase.GetItem(movieId, language);

                    if (movItm != null)
                        mediaBrowserOptions.SelectedItem = movItm;
                }

                SheerResponse.ShowModalDialog(mediaBrowserOptions.ToUrlString().ToString(), "1200px", "700px", string.Empty, true);
                args.WaitForPostBack();
            }
        }
    }
}