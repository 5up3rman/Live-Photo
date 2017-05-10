using System;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.Configuration.Attributes;
using Sitecore.Globalization;

namespace Paragon.Foundation.Models
{
    [SitecoreType(TemplateId= "{B3B1C57C-236C-4428-A745-6F556CA1D506}")]
    public interface IPageBase
    {
        [SitecoreId]
        Guid Id { get; }

        [SitecoreInfo(SitecoreInfoType.Language)]
        Language Language { get; }

        [SitecoreInfo(SitecoreInfoType.Version)]
        int Version { get; }

        [SitecoreInfo(SitecoreInfoType.Url)]
        string Url { get; }
    }
}