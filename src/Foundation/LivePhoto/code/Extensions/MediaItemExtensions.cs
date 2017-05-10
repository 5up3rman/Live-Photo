using System;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;

namespace Paragon.Foundation.LivePhoto.Extensions
{
    public static class MediaItemExtensions
    {
        public static int GetImageRatioHeight(Database db, ID id, int maxHeight = 440, int maxWidth = 460)
        {
            var image = (MediaItem) db.GetItem(id);

            if (image == null)
                return maxHeight;

            var originalHeight = image.ImageHeight();
            var originalWidth = image.ImageWidth();
            var ratioX = maxHeight/(double) originalHeight;
            var ratioY = maxWidth/(double) originalWidth;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            return Convert.ToInt32(originalHeight*ratio);
        }

        public static int GetImageRatioWidth(Database db, ID id, int maxHeight = 440, int maxWidth = 460)
        {
            var image = (MediaItem) db.GetItem(id);

            if (image == null)
                return maxWidth;

            var originalHeight = image.ImageHeight();
            var originalWidth = image.ImageWidth();
            var ratioX = maxHeight/(double) originalHeight;
            var ratioY = maxWidth/(double) originalWidth;
            var ratio = ratioX < ratioY ? ratioX : ratioY;

            return Convert.ToInt32(originalWidth*ratio);
        }

        public static int ImageHeight(this MediaItem mi, int maxHeight = 440)
        {
            if (mi == null)
                return maxHeight;

            var dimensions = mi.GetMetaData().FirstOrDefault(i => i.Key.Equals("Dimensions")).Value.Split('x').ToArray()[1] ?? maxHeight.ToString();
            int.TryParse(dimensions, out maxHeight);

            return maxHeight;
        }

        public static int ImageWidth(this MediaItem mi, int maxWidth = 460)
        {
            if (mi == null)
                return maxWidth;

            var dimensions = mi.GetMetaData().FirstOrDefault(i => i.Key.Equals("Dimensions")).Value.Split('x').ToArray()[0] ?? maxWidth.ToString();
            int.TryParse(dimensions, out maxWidth);

            return maxWidth;
        }
    }
}