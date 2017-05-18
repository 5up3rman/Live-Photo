using Sitecore;
using Sitecore.IO;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp;

namespace Paragon.Foundation.LivePhoto.Commands.WebEdit
{
    public class SetImageProperties : WebEditImageCommand
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
            var xmlValue = new XmlValue(args.Parameters["fieldValue"], "image");
            var str = args.Parameters["controlid"];

            if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;
                
                SheerResponse.SetAttribute("scHtmlValue", "value", RenderImage(args, args.Result));
                SheerResponse.SetAttribute("scPlainValue", "value", args.Result);
                SheerResponse.Eval($"scSetHtmlValue('{str}')");
            }
            else
            {
                var urlString = new UrlString(FileUtil.MakePath("/sitecore/shell", ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Media.ImageProperties"))));
                var mediaId = xmlValue.GetAttribute("mediaid");

                if (string.IsNullOrEmpty(mediaId))
                    return;

                var mediaItm = Client.ContentDatabase.GetItem(mediaId);

                if (mediaItm == null)
                {
                    SheerResponse.Alert("Select an image from the Media Library first.");
                }
                else
                {
                    mediaItm.Uri.AddToUrlString(urlString);

                    // Must clear all dimensions or Keep Aspect Ratio and Width controls will be disabled on the modal.
                    xmlValue.SetAttribute("h", string.Empty);
                    xmlValue.SetAttribute("w", string.Empty);
                    xmlValue.SetAttribute("height", string.Empty);
                    xmlValue.SetAttribute("width", string.Empty);

                    var urlHandle = new UrlHandle { ["xmlvalue"] = xmlValue.ToString() };
                    urlHandle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), true);
                    args.WaitForPostBack();
                }
            }
        }
    }
}