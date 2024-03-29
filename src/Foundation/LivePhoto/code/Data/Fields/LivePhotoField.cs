﻿using System;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Mvc.Extensions;
using Version = Sitecore.Data.Version;

namespace Paragon.Foundation.LivePhoto.Data.Fields
{
    /// <summary>
    /// Represents an Live Photo field.
    /// Based on Sitecore.Data.Fields.ImageField
    /// </summary>
    public class LivePhotoField : XmlField
    {
        private Database mediaDatabase;
        private Item photoMediaItem;
        private Item movieMediaItem;
        private Language mediaLanguage;

        public LivePhotoField(Field innerField) : base(innerField, "livephoto")
        {
        }

        public LivePhotoField(Field innerField, string runtimeValue) : base(innerField, "livephoto", runtimeValue)
        {
        }

        public static implicit operator LivePhotoField(Field field)
        {
            return field != null ? new LivePhotoField(field) : null;
        }

        public string Height
        {
            get
            {
                var attribute = GetAttribute("height");
                var mediaItem = PhotoMediaItem;

                if (attribute.Length > 0)
                    return attribute;

                return mediaItem != null ? mediaItem["height"] : string.Empty;
            }
            set { SetAttribute("height", value); }
        }

        public string Width
        {
            get
            {
                var attribute = GetAttribute("width");
                var mediaItem = PhotoMediaItem;

                if (attribute.Length > 0)
                    return attribute;

                return mediaItem != null ? mediaItem["width"] : string.Empty;
            }
            set { SetAttribute("width", value); }
        }

        public ID MovieId
        {
            get
            {
                var attribute = GetAttribute("movieid");
                return ID.IsID(attribute) ? ID.Parse(attribute) : ID.Null;
            }
            set { SetAttribute("movieid", value.ToString()); }
        }

        public ID MediaId
        {
            get
            {
                var attribute = GetAttribute("mediaid");
                return ID.IsID(attribute) ? ID.Parse(attribute) : ID.Null;
            }
            set { SetAttribute("mediaIid", value.ToString()); }
        }

        public bool ShowPhotoTime
        {
            get
            {
                return GetAttribute("data-photo-time").ToBool();
            }
            set
            {
                SetAttribute("data-photo-time", value.ToString());
            }
        }

        public bool ShowControls
        {
            get
            {
                return GetAttribute("data-show-native-controls").ToBool();
            }
            set
            {
                SetAttribute("data-show-native-controls", value.ToString());
            }
        }

        public bool ProactivelyLoadsVideo
        {
            get
            {
                return GetAttribute("data-proactively-loads-video").ToBool();
            }
            set
            {
                SetAttribute("data-proactively-loads-video", value.ToString());
            }
        }

        public Item MovieMediaItem
        {
            get
            {
                if (movieMediaItem != null)
                    return movieMediaItem;

                movieMediaItem = MediaDatabase.GetItem(MovieId, MediaLanguage, MediaVersion);

                return movieMediaItem;
            }
        }

        public Item PhotoMediaItem
        {
            get
            {
                if (photoMediaItem != null)
                    return photoMediaItem;

                photoMediaItem = MediaDatabase.GetItem(MediaId, MediaLanguage, MediaVersion);

                return photoMediaItem;
            }
        }

        public Database MediaDatabase
        {
            get
            {
                if (mediaDatabase != null)
                    return mediaDatabase;

                mediaDatabase = InnerField.Item.Database;

                return mediaDatabase;
            }
        }

        public Language MediaLanguage
        {
            get
            {
                if (mediaLanguage != null)
                    return mediaLanguage;

                mediaLanguage = InnerField.Item.Language;

                return mediaLanguage;
            }
        }

        public Version MediaVersion { get; set; } = Version.Latest;

        public void Clear()
        {
            Value = string.Empty;
        }
    }
}