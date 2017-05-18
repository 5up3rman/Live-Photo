using Paragon.Foundation.LivePhoto.Extensions;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines.RenderField;
using Sitecore.Shell.Applications.ContentEditor;

namespace Paragon.Foundation.LivePhoto.Pipelines.RenderField
{
    public class GetLivePhotoValue
    {
        public void Process(RenderFieldArgs args)
        {
            if (args.FieldTypeKey != "livephoto")
                return;

            var renderer = CreateRenderer();
            renderer.Item = args.Item;
            renderer.FieldName = args.FieldName;
            renderer.FieldValue = args.FieldValue;

            var parameters = args.Parameters;
            var mediaItem = GetMediaItem(args.Item.Database, args.FieldValue);

            if (mediaItem != null)
            {
                if (!parameters.ContainsKey("height"))
                    parameters.Add("height", mediaItem.ImageHeight().ToString());

                if (!parameters.ContainsKey("width"))
                    parameters.Add("width", mediaItem.ImageWidth().ToString());
            }

            renderer.Parameters = parameters;
            args.WebEditParameters.AddRange(args.Parameters);
            renderer.Parameters.Add("la", args.Item.Language.Name);

            var renderFieldResult = renderer.Render(args.Before);
            args.Result.FirstPart = renderFieldResult.FirstPart;
            args.Result.LastPart = renderFieldResult.LastPart;
            args.DisableWebEditContentEditing = true;
            args.DisableWebEditFieldWrapping = true;
            args.WebEditClick = "return Sitecore.WebEdit.editControl($JavascriptParameters, 'paragon:ChooseLivePhotoMovie')";
        }

        private MediaItem GetMediaItem(Database db, string fieldValue)
        {
            var xmlValue = new XmlValue(fieldValue, "image");
            var mediaId = xmlValue.GetAttribute("mediaid");

            if (string.IsNullOrEmpty(mediaId) || !ID.IsID(mediaId))
                return null;

            return db.GetItem(new ID(mediaId));
        }

        private LivePhotoRenderer CreateRenderer()
        {
            return new LivePhotoRenderer();
        }
    }
}