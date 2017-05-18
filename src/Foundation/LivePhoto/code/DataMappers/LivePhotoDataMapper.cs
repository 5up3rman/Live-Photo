using System;
using Glass.Mapper;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.Configuration;
using Glass.Mapper.Sc.DataMappers;
using Paragon.Foundation.LivePhoto.Data.Fields;
using Paragon.Foundation.LivePhoto.Extensions;
using Paragon.Foundation.LivePhoto.GlassFieldObjects;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor;

namespace Paragon.Foundation.LivePhoto.DataMappers
{
    public class LivePhotoDataMapper : AbstractSitecoreFieldMapper
    {
        public LivePhotoDataMapper() : base(typeof (LivePhotoGlassObject))
        {
        }
        
        #region Get Field Value

        public override object GetFieldValue(string fieldValue, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            if (string.IsNullOrEmpty(fieldValue))
                return null;

            var livePhotoGlassObj = new LivePhotoGlassObject();
            MapLivePhoto(context, livePhotoGlassObj, fieldValue);
            return livePhotoGlassObj;
        }

        private static void MapLivePhoto(SitecoreDataMappingContext context, ILivePhotoGlassObject livePhotoGlassObj, string fieldValue)
        {
            if (string.IsNullOrWhiteSpace(fieldValue))
                return;

            var xmlValue = new XmlValue(fieldValue, "image");

            if (string.IsNullOrEmpty(xmlValue.GetAttribute("mediaid")))
                return;

            var mediaId = new ID(xmlValue.GetAttribute("mediaid"));

            if (ID.IsNullOrEmpty(mediaId))
                return;

            var mediaItem = (MediaItem) context.Service.Database.GetItem(mediaId);

            if (mediaItem == null)
                return;

            // Dimensions 
            var height = xmlValue.GetAttribute("height").ToInt();

            if (height == 0)
                height = mediaItem.ImageHeight();

            var width = xmlValue.GetAttribute("width").ToInt();

            if (width == 0)
                width = mediaItem.ImageWidth();

            livePhotoGlassObj.Height = height;
            livePhotoGlassObj.Width = width;
            livePhotoGlassObj.MediaId = mediaId.ToGuid();
            livePhotoGlassObj.DataPhotoSrc = MediaManager.GetMediaUrl(mediaItem);
            livePhotoGlassObj.InlineStyle = $"\"height:{height}px; width:{width}px;\"";

            if (string.IsNullOrEmpty(xmlValue.GetAttribute("movieid")))
                return;

            var movieId = new ID(xmlValue.GetAttribute("movieid"));

            if (ID.IsNullOrEmpty(movieId))
                return;

            var movieItem = (MediaItem) context.Service.Database.GetItem(movieId);

            if (movieItem == null)
                return;

            livePhotoGlassObj.MovieId = movieId.ToGuid();
            livePhotoGlassObj.DataVideoSrc = MediaManager.GetMediaUrl(movieItem);

            // Live Photo options
            var photoTime = xmlValue.GetAttribute("data-photo-time");
            var proactivelyLoadVideo = xmlValue.GetAttribute("data-proactively-loads-video");
            var showNativeControls = xmlValue.GetAttribute("data-shows-native-controls");

            if (!string.IsNullOrEmpty(photoTime))
                livePhotoGlassObj.DataPhotoTime = "data-photo-time";

            if (!string.IsNullOrEmpty(proactivelyLoadVideo))
                livePhotoGlassObj.DataProactivelyLoadsVideo = "data-proactively-loads-video";

            if (!string.IsNullOrEmpty(showNativeControls))
                livePhotoGlassObj.DataShowsNativeControls = "data-shows-native-controls";
        }

        #endregion Get Field Value

        #region Set Field

        public override string SetFieldValue(object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            throw new NotImplementedException();
        }

        public override void SetField(Field field, object value, SitecoreFieldConfiguration config, SitecoreDataMappingContext context)
        {
            var livePhoto = value as LivePhotoGlassObject;
            var itm = field?.Item;

            if (itm == null)
                return;

            MapToField(new LivePhotoField(field), livePhoto, itm);
        }

        public static void MapToField(LivePhotoField field, LivePhotoGlassObject livePhoto, Item item)
        {
            if (livePhoto == null)
            {
                field.Clear();
            }
            else
            {
                // Photo Media
                if (!field.MediaId.Guid.Equals(livePhoto.MediaId))
                {
                    if (livePhoto.MediaId.Equals(Guid.Empty))
                    {
                        var itemLink = new ItemLink(item.Database.Name, item.ID, field.InnerField.ID, field.PhotoMediaItem.Database.Name, field.MediaId, field.PhotoMediaItem.Paths.Path);
                        field.RemoveLink(itemLink);
                    }
                    else
                    {
                        var photoMediaId = new ID(livePhoto.MediaId);
                        var photoItm = item.Database.GetItem(photoMediaId);
                        if (photoItm != null)
                        {
                            field.MediaId = photoMediaId;
                            var itemLink = new ItemLink(item.Database.Name, item.ID, field.InnerField.ID, photoItm.Database.Name, photoItm.ID, photoItm.Paths.FullPath);
                            field.UpdateLink(itemLink);
                        }
                        else
                            throw new MapperException($"No item with ID {photoMediaId}. Can not update Media Item field");
                    }
                }

                // Movie Media
                if (!field.MovieId.Guid.Equals(livePhoto.MovieId))
                {
                    if (livePhoto.MovieId.Equals(Guid.Empty))
                    {
                        var itemLink = new ItemLink(item.Database.Name, item.ID, field.InnerField.ID, field.PhotoMediaItem.Database.Name, field.MediaId, field.PhotoMediaItem.Paths.Path);
                        field.RemoveLink(itemLink);
                    }
                    else
                    {
                        var movieMediaId = new ID(livePhoto.MediaId);
                        var movieItm = item.Database.GetItem(movieMediaId);
                        if (movieItm != null)
                        {
                            field.MovieId = movieMediaId;
                            var itemLink = new ItemLink(item.Database.Name, item.ID, field.InnerField.ID, movieItm.Database.Name, movieItm.ID, movieItm.Paths.FullPath);
                            field.UpdateLink(itemLink);
                        }
                        else
                            throw new MapperException($"No item with ID {movieMediaId}. Can not update Movie Media Item field");
                    }
                }

                if (livePhoto.Height > 0)
                    field.Height = livePhoto.Height.ToString();
                if (livePhoto.Width > 0)
                    field.Width = livePhoto.Width.ToString();
            }
        }

        #endregion Set Field
    }
}