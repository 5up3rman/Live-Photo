using System;
using Glass.Mapper;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.DataMappers;
using Paragon.Foundation.LivePhoto.Extensions;
using Paragon.Foundation.LivePhoto.Models;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor;

namespace Paragon.Foundation.LivePhoto.DataMappers
{
    public class LivePhotoDataMapper : AbstractSitecoreFieldMapper
    {
        public LivePhotoDataMapper() : base(typeof (LivePhotoObject))
        {
        }

        public override string SetFieldValue(object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            throw new NotImplementedException();
        }

        public override void SetField(Field field, object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            
        }

        public override object GetFieldValue(string fieldValue, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return null;

            var xmlValue = new XmlValue(fieldValue, "image");
            var mediaId = xmlValue.GetAttribute("mediaid");
            var movieId = xmlValue.GetAttribute("movieid");

            if (!ID.IsID(mediaId) || !ID.IsID(movieId))
                return null;

            var livePhotoObject = new LivePhotoObject();

            // Live Photo options
            var photoTime = xmlValue.GetAttribute("data-photo-time");
            var proactivelyLoadVideo = xmlValue.GetAttribute("data-proactively-loads-video");
            var showNativeControls = xmlValue.GetAttribute("data-shows-native-controls");

            var mediaItem = (MediaItem) context.Service.Database.GetItem(new ID(mediaId));
            var movieItem = (MediaItem) context.Service.Database.GetItem(new ID(movieId));

            if (mediaItem == null || movieItem == null)
                return null;
            
            if (!string.IsNullOrEmpty(photoTime))
                livePhotoObject.DataPhotoTime = "data-photo-time";

            if (!string.IsNullOrEmpty(proactivelyLoadVideo))
                livePhotoObject.DataProactivelyLoadsVideo = "data-proactively-loads-video";

            if (!string.IsNullOrEmpty(showNativeControls))
                livePhotoObject.DataShowsNativeControls = "data-shows-native-controls";

            // Dimensions 
            var height = xmlValue.GetAttribute("height").ToInt();

            if (height == 0)
                height = mediaItem.ImageHeight();

            var width = xmlValue.GetAttribute("width").ToInt();

            if (width == 0)
                width = mediaItem.ImageWidth();

            livePhotoObject.Height = height;
            livePhotoObject.Width = width;
            livePhotoObject.MovieId = Guid.Parse(movieId);
            livePhotoObject.MediaId = Guid.Parse(mediaId);
            livePhotoObject.DataPhotoSrc = MediaManager.GetMediaUrl(mediaItem);
            livePhotoObject.DataVideoSrc = MediaManager.GetMediaUrl(movieItem);
            livePhotoObject.InlineStyle = $"\"height:{height}px; width:{width}px;\"";

            return livePhotoObject;
        }
    }
}