using Glass.Mapper.Sc.Configuration.Attributes;
using Paragon.Foundation.LivePhoto.GlassFieldObjects;

namespace Paragon.Foundation.Models
{
    [SitecoreType(TemplateId = "{A4356968-97D7-4135-8AF5-666173CAE69D}", AutoMap = true)]
    public interface ILivePhotoDemo: IPageBase
    {
        [SitecoreField(FieldId = "{5C4B16DC-0FC4-4211-AAEA-6BB399E67601}")]
        LivePhotoGlassObject Photo { get; set; }
    }
}