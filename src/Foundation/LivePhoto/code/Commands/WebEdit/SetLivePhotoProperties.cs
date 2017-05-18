
using Sitecore;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace Paragon.Foundation.LivePhoto.Commands.WebEdit
{
    public class SetLivePhotoProperties : WebEditImageCommand
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
                var xmlValue = new XmlValue(args.Parameters["fieldValue"], "image");
                var urlString = new UrlString("/sitecore/shell/~/xaml/Sitecore.Shell.Applications.Media.ShowLivePhoto.aspx");
                var urlHandle = new UrlHandle { ["xmlvalue"] = xmlValue.ToString() };
                urlHandle.Add(urlString);

                SheerResponse.ShowModalDialog(urlString.ToString(), true);
                args.WaitForPostBack();
            }
        }
    }
}