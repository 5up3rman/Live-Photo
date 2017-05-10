using System;
using Sitecore.Globalization;

namespace Paragon.Foundation.LivePhoto.Models
{
    [Serializable]
    public class LivePhotoObject : ILivePhotoObject
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public Guid MediaId { get; set; }
        public Guid MovieId { get; set; }
        public string DataLivePhoto { get;set; } = "data-live-photo";
        public string DataPhotoSrc { get; set; }
        public string DataVideoSrc { get; set; }
        public string InlineStyle { get; set; }
        public string DataPhotoTime { get; set; }
        public string DataProactivelyLoadsVideo { get; set; }
        public string DataShowsNativeControls { get; set; }
    }
}