using System;
using Sitecore.Globalization;

namespace Paragon.Foundation.LivePhoto.GlassFieldObjects
{
    public interface ILivePhotoGlassObject
    {
        string DataLivePhoto { get; set; }
        string DataPhotoSrc { get; set; }
        string DataPhotoTime { get; set; }
        string DataProactivelyLoadsVideo { get; set; }
        string DataShowsNativeControls { get; set; }
        string DataVideoSrc { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        string InlineStyle { get; set; }
        Guid MediaId { get; set; }
        Guid MovieId { get; set; }
        Language Language { get; set; }
    }
}