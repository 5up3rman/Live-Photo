using Sitecore.Pipelines.RenderField;

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
            renderer.Parameters = args.Parameters;
            args.WebEditParameters.AddRange(args.Parameters);
            renderer.Parameters.Add("la", args.Item.Language.Name);

            var renderFieldResult = renderer.Render(args.Before);
            args.Result.FirstPart = renderFieldResult.FirstPart;
            args.Result.LastPart = renderFieldResult.LastPart;
            args.DisableWebEditContentEditing = true;
            args.DisableWebEditFieldWrapping = true;
            args.WebEditClick = "return Sitecore.WebEdit.editControl($JavascriptParameters, 'paragon:ChooseLivePhotoMovie')";
        }

        private LivePhotoRenderer CreateRenderer()
        {
            return new LivePhotoRenderer();
        }
    }
}