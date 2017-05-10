using System;

namespace Paragon.Foundation.LivePhoto.Models
{
    public interface ILivePhotoObject
    {
        string DataLivePhoto { get; set; }
        string DataPhotoSrc { get; set; }
        string DataPhotoTime { get; set; }
        string DataProactivelyLoadsVideo { get; set; }
        string DataShowsNativeControls { get; set; }
        string DataVideoSrc { get; set; }
        int Height { get; set; }
        string InlineStyle { get; set; }
        Guid MediaId { get; set; }
        Guid MovieId { get; set; }
        int Width { get; set; }
    }
}