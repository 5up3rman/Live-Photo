using System;
using System.Web.UI.WebControls;
using Paragon.Foundation.LivePhoto.Extensions;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Controls;
using Sitecore.Data;
using Sitecore.Mvc.Extensions;
using Sitecore.Resources.Media;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace Paragon.Foundation.LivePhoto.Shell.Applications.Media.LivePhoto
{
    public class ShowLivePhotoPage : DialogPage
    {
        public Literal LivePhotoContainer;
        public CheckBox ShowPhotoTime;
        public CheckBox ProactivelyLoadVideo;
        public CheckBox ShowPlaybackControls;

        public const int maxHeight = 440;
        public const int maxWidth = 460;

        private XmlValue XmlValue
        {
            get { return new XmlValue(StringUtil.GetString(ViewState["XmlValue"]), "image"); }
            set { ViewState["XmlValue"] = value.ToString(); }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (AjaxScriptManager.IsEvent)
                return;

            var urlHandle = UrlHandle.Get();
            XmlValue = new XmlValue(urlHandle["xmlvalue"], "image");

            var db = Factory.GetDatabase(WebUtil.GetQueryString("db", "master"));
            var img = db.GetItem(new ID(GetMediaId(XmlValue)));
            var mov = db.GetItem(new ID(GetMovieId(XmlValue)));

            if (img == null || mov == null)
                return;

            var height = MediaItemExtensions.GetImageRatioHeight(db, img.ID);
            var width = MediaItemExtensions.GetImageRatioWidth(db, img.ID);

            var showDataPhotoTime = XmlValue.GetAttribute("data-photo-time").ToBool();
            ShowPhotoTime.Checked = showDataPhotoTime;

            var proactivelyLoadVideo = XmlValue.GetAttribute("data-proactively-loads-video").ToBool();
            ProactivelyLoadVideo.Checked = proactivelyLoadVideo;

            var showPlaybackControls = XmlValue.GetAttribute("data-shows-native-controls").ToBool();
            ShowPlaybackControls.Checked = showPlaybackControls;

            LivePhotoContainer.Text = $"<span data-live-photo data-photo-src=\"{MediaManager.GetMediaUrl(img)}\" data-video-src=\"{MediaManager.GetMediaUrl(mov)}\" data-proactively-loads-video=\"{proactivelyLoadVideo.ToString().ToLower()}\" data-photo-time=\"{showDataPhotoTime.ToString().ToLower()}\" data-shows-native-controls=\"{showPlaybackControls.ToString().ToLower()}\" style=\"width:{width}px; height:{height}px;\"></span>";
        }

        protected override void OK_Click()
        {
            var xml = XmlValue;
            xml.SetAttribute("data-photo-time", ShowPhotoTime.Checked.ToString().ToLower());
            xml.SetAttribute("data-proactively-loads-video", ProactivelyLoadVideo.Checked.ToString().ToLower());
            xml.SetAttribute("data-shows-native-controls", ShowPlaybackControls.Checked.ToString().ToLower());

            SheerResponse.SetDialogValue(xml.ToString());
            base.OK_Click();
        }

        private string GetMediaId(XmlValue val)
        {
            return val.GetAttribute("mediaid");
        }

        private string GetMovieId(XmlValue val)
        {
            return val.GetAttribute("movieid");
        }
    }
}