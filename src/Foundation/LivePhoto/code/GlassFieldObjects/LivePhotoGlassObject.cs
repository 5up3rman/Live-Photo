using System;
using Sitecore.Globalization;

namespace Paragon.Foundation.LivePhoto.GlassFieldObjects
{
    /// <summary>
    /// Based on Glass.Mapper.Sc.Fields.Image
    /// </summary>
    [Serializable]
    public class LivePhotoGlassObject : ILivePhotoGlassObject
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public Guid MediaId { get; set; }
        public Guid MovieId { get; set; }
        public Language Language { get; set; }
        public string DataLivePhoto { get;set; } = "data-live-photo";
        public string DataPhotoSrc { get; set; }
        public string DataVideoSrc { get; set; }
        public string InlineStyle { get; set; }
        public string DataPhotoTime { get; set; }
        public string DataProactivelyLoadsVideo { get; set; }
        public string DataShowsNativeControls { get; set; }
    }
}