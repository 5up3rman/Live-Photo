using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Exceptions;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Resources.Media;
using Sitecore.Shell;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Shell.Framework;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XamlSharp;
using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using Sitecore.Shell.Applications.ContentEditor;

namespace Paragon.Foundation.LivePhoto.ContentEditor
{
    public class LivePhoto : LinkBase
    {
        public string ItemVersion
        {
            get { return GetViewStateString("Version"); }
            set { SetViewStateString("Version", value); }
        }

        protected XmlValue XmlValue
        {
            get
            {
                var xmlValue = GetViewStateProperty("XmlValue", null) as XmlValue;

                if (xmlValue != null)
                    return xmlValue;

                xmlValue = new XmlValue(string.Empty, "livephoto");
                XmlValue = xmlValue;

                return xmlValue;
            }
            set { SetViewStateProperty("XmlValue", value, null); }
        }

        public LivePhoto()
        {
            Class = "scContentControlImage";
            Change = "#";
            Activation = true;
        }

        public override string GetValue()
        {
            return XmlValue.ToString();
        }

        public string AssociatedMovieValue
        {
            get { return GetViewStateString("MovieValue"); }
            set
            {
                if (value == AssociatedMovieValue)
                    return;

                SetViewStateString("MovieValue", value);
                SheerResponse.SetAttribute(ID, "movievalue", value);
                Attributes["movievalue"] = value;
            }
        }

        public override void HandleMessage(Message message)
        {
            base.HandleMessage(message);

            if (message["id"] != ID)
                return;

            switch (message.Name)
            {
                case "contentimage:open":
                    Browse();
                    break;
                case "contentimage:livephoto":
                    BrowseMovie();
                    break;
                case "contentimage:ShowLivePhotoProperties":
                    Sitecore.Context.ClientPage.Start(this, "ShowLivePhotoProperties");
                    break;
                case "contentimage:properties":
                    Sitecore.Context.ClientPage.Start(this, "ShowProperties");
                    break;
                case "contentimage:edit":
                    Edit();
                    break;
                case "contentimage:load":
                    LoadImage();
                    break;
                case "contentimage:clear":
                    ClearImage();
                    break;
                case "contentimage:refresh":
                    Update();
                    break;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ServerProperties["Value"] = ServerProperties["Value"];
            ServerProperties["XmlValue"] = ServerProperties["XmlValue"];
            ServerProperties["Language"] = ServerProperties["Language"];
            ServerProperties["Version"] = ServerProperties["Version"];
            ServerProperties["Source"] = ServerProperties["Source"];
            ServerProperties["MovieSource"] = ServerProperties["MovieSource"];

            var d = 1;
        }

        protected void Browse()
        {
            if (Disabled)
                return;

            Sitecore.Context.ClientPage.Start(this, "BrowseImage");
        }

        protected void BrowseMovie()
        {
            if (Disabled)
                return;

            Sitecore.Context.ClientPage.Start(this, "BrowseAssociatedMovie");
        }

        protected void BrowseImage(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (string.IsNullOrEmpty(args.Result) || args.Result == "undefined")
                    return;

                var mediaItem = (MediaItem) Client.ContentDatabase.Items[args.Result];

                if (mediaItem != null)
                {
                    var template = mediaItem.InnerItem.Template;
                    if (template != null && !IsImageMedia(template))
                    {
                        SheerResponse.Alert("The selected item does not contain an image.");
                    }
                    else
                    {
                        XmlValue.SetAttribute("mediaid", mediaItem.ID.ToString());
                        Value = mediaItem.MediaPath;
                        Update();
                        SetModified();
                    }
                }
                else
                    SheerResponse.Alert("Item not found.");
            }
            else
            {
                var str1 = StringUtil.GetString(new string[2]
                {
                    Source,
                    "/sitecore/media library"
                });
                var str2 = str1;
                var path = XmlValue.GetAttribute("mediaid");
                var str3 = path;
                if (str1.StartsWith("~", StringComparison.InvariantCulture))
                {
                    str2 = StringUtil.Mid(str1, 1);
                    if (string.IsNullOrEmpty(path))
                        path = str2;
                    str1 = "/sitecore/media library";
                }
                var language = Language.Parse(ItemLanguage);
                var mediaBrowserOptions = new MediaBrowserOptions();
                var obj1 = Client.ContentDatabase.GetItem(str1, language);
                if (obj1 == null)
                    throw new ClientAlertException("The source of this Image field points to an item that does not exist.");
                mediaBrowserOptions.Root = obj1;
                if (!string.IsNullOrEmpty(path))
                {
                    var obj2 = Client.ContentDatabase.GetItem(path, language);
                    if (obj2 != null)
                        mediaBrowserOptions.SelectedItem = obj2;
                }
                var urlHandle = new UrlHandle
                {
                    ["ro"] = str1,
                    ["fo"] = str2,
                    ["db"] = Client.ContentDatabase.Name,
                    ["la"] = ItemLanguage,
                    ["va"] = str3
                };
                var urlString = mediaBrowserOptions.ToUrlString();
                urlHandle.Add(urlString);
                SheerResponse.ShowModalDialog(urlString.ToString(), "1200px", "700px", string.Empty, true);
                args.WaitForPostBack();
            }
        }

        protected void BrowseAssociatedMovie(ClientPipelineArgs args)
        {
            if (args.IsPostBack)
            {
                if (string.IsNullOrEmpty(args.Result) || args.Result == "undefined")
                    return;

                var mediaItem = (MediaItem) Client.ContentDatabase.Items[args.Result];

                if (mediaItem != null)
                {
                    var shellOptions = MediaUrlOptions.GetShellOptions();
                    var mediaUrl = MediaManager.GetMediaUrl(mediaItem, shellOptions);
                    XmlValue.SetAttribute("movieid", mediaItem.ID.ToString());
                    AssociatedMovieValue = mediaItem.MediaPath;
                    SetModified();
                }
                else
                    Sitecore.Context.ClientPage.ClientResponse.Alert("Item not found.");
            }
            else
            {
                Dialogs.BrowseImage(XmlValue.GetAttribute("movieid"), StringUtil.GetString(new string[2]
                {MovieSource, "/sitecore/media library"}), true);
                args.WaitForPostBack();
            }
        }

        protected void LoadImage()
        {
            var attribute = XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.");
            }
            else
            {
                if (!UserOptions.View.ShowEntireTree)
                {
                    var obj1 = Client.CoreDatabase.GetItem("/sitecore/content/Applications/Content Editor/Applications/MediaLibraryForm");
                    if (obj1 != null)
                    {
                        var obj2 = Client.ContentDatabase.GetItem(attribute);
                        if (obj2 != null)
                        {
                            var urlString = new UrlString(obj1["Source"])
                            {
                                ["pa"] = "1",
                                ["pa0"] = WebUtil.GetQueryString("pa0", string.Empty),
                                ["la"] = WebUtil.GetQueryString("la", string.Empty),
                                ["pa1"] = HttpUtility.UrlEncode(obj2.Uri.ToString())
                            };
                            SheerResponse.SetLocation(urlString.ToString());
                            return;
                        }
                    }
                }
                var language = Language.Parse(ItemLanguage);
                Sitecore.Context.ClientPage.SendMessage(this, "item:load(id=" + attribute + ",language=" + language.Name + ")");
            }
        }

        protected override void DoChange(Message message)
        {
            base.DoChange(message);

            if (string.IsNullOrEmpty(Value))
            {
                ClearImage();
            }
            else
            {
                var path = Value;
                if (!path.StartsWith("/sitecore", StringComparison.InvariantCulture))
                    path = "/sitecore/media library" + path;
                var mediaItem = (MediaItem) Client.ContentDatabase.GetItem(path, Language.Parse(ItemLanguage));
                if (mediaItem != null)
                    SetValue(mediaItem);
                else
                    SetValue(string.Empty);
                Update();
                SetModified();
            }

            SheerResponse.SetReturnValue(true);
        }

        protected override void DoRender(HtmlTextWriter output)
        {
            var mediaItem = GetMediaItem();
            string src;
            GetSrc(out src);
            var imgSrc = " src=\"" + src + "\"";
            var imgId = " id=\"" + ID + "_image\"";
            var imgAlt = " alt=\"" + (mediaItem != null ? HttpUtility.HtmlEncode(mediaItem["Alt"]) : string.Empty) + "\"";
            base.DoRender(output);
            output.Write("<div id=\"" + ID + "_pane\" class=\"scContentControlImagePane\">");
            var clientEvent = Sitecore.Context.ClientPage.GetClientEvent(ID + ".Browse");
            output.Write("<div class=\"scContentControlImageImage\" onclick=\"" + clientEvent + "\">");
            output.Write("<iframe" + imgId + imgSrc + imgAlt + " frameborder=\"0\" marginwidth=\"0\" marginheight=\"0\" width=\"100%\" height=\"128\" allowtransparency=\"allowtransparency\"></iframe>");
            output.Write("</div>");
            output.Write("<div id=\"" + ID + "_details\" class=\"scContentControlImageDetails\">");
            var details = GetDetails();
            output.Write(details);
            output.Write("</div>");
            output.Write("</div>");
        }

        protected override void SetModified()
        {
            base.SetModified();
            if (!TrackModified)
                return;

            Sitecore.Context.ClientPage.Modified = true;
        }

        protected void ShowProperties(ClientPipelineArgs args)
        {
            if (Disabled)
                return;

            var mediaId = XmlValue.GetAttribute("mediaid");

            if (string.IsNullOrEmpty(mediaId))
                SheerResponse.Alert("Select an image from the Media Library first.");

            else if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;

                XmlValue = new XmlValue(args.Result, "image");
                Value = GetMediaPath();
                SetModified();
                Update();
            }
            else
            {
                var urlString = new UrlString(FileUtil.MakePath("/sitecore/shell", ControlManager.GetControlUrl(new ControlName("Sitecore.Shell.Applications.Media.ImageProperties"))));
                var mediaItm = Client.ContentDatabase.GetItem(mediaId, Language.Parse(ItemLanguage));

                if (mediaItm == null)
                {
                    SheerResponse.Alert("Select an image from the Media Library first.");
                }
                else
                {
                    mediaItm.Uri.AddToUrlString(urlString);
                    var urlHandle = new UrlHandle {["xmlvalue"] = XmlValue.ToString()};
                    urlHandle.Add(urlString);
                    SheerResponse.ShowModalDialog(urlString.ToString(), true);
                    args.WaitForPostBack();
                }
            }
        }

        protected void ShowLivePhotoProperties(ClientPipelineArgs args)
        {
            if (Disabled)
                return;

            var movieId = XmlValue.GetAttribute("movieid");

            if (string.IsNullOrEmpty(movieId))
                SheerResponse.Alert("Select a movie file from the Media Library first.");

            else if (args.IsPostBack)
            {
                if (!args.HasResult)
                    return;

                XmlValue = new XmlValue(args.Result, "image");
                AssociatedMovieValue = GetMoviePath();
                SetModified();
            }
            else
            {
                var urlString = new UrlString(FileUtil.MakePath("/sitecore/shell", ControlManager.GetControlUrl(new ControlName("Sitecore.shell.Applications.Media.ShowLivePhoto"))));
                var movieItm = Client.ContentDatabase.GetItem(movieId, Language.Parse(ItemLanguage));

                if (movieItm == null)
                {
                    SheerResponse.Alert("Select an movie file from the Media Library first.");
                }
                else
                {
                    movieItm.Uri.AddToUrlString(urlString);
                    var urlHandle = new UrlHandle {["xmlvalue"] = XmlValue.ToString()};
                    urlHandle.Add(urlString);

                    SheerResponse.ShowModalDialog(urlString.ToString(), true);
                    args.WaitForPostBack();
                }
            }
        }

        protected void Edit()
        {
            var attribute = XmlValue.GetAttribute("mediaid");

            if (string.IsNullOrEmpty(attribute))
            {
                SheerResponse.Alert("Select an image from the Media Library first.");
            }
            else
            {
                var innerItem = Client.ContentDatabase.GetItem(attribute, Language.Parse(ItemLanguage));
                if (innerItem == null)
                    SheerResponse.Alert("Select an image from the Media Library first.");
                else if (new MediaItem(innerItem).MimeType.ToLower() == "image/svg+xml")
                {
                    SheerResponse.Alert("Editing SVG images is unsupported.");
                }
                else
                {
                    if (Disabled)
                        return;

                    Sitecore.Context.ClientPage.Start(this, "EditImage");
                }
            }
        }

        protected void EditImage(ClientPipelineArgs args)
        {
            var mediaId = XmlValue.GetAttribute("mediaid");
            if (string.IsNullOrEmpty(mediaId))
            {
                SheerResponse.Alert("Select an image from the Media Library first.");
            }
            else
            {
                if (args.IsPostBack)
                {
                    if (args.Result != "yes")
                    {
                        args.AbortPipeline();
                        return;
                    }
                }
                else
                {
                    var mediaItem = Client.ContentDatabase.GetItem(mediaId);

                    if (mediaItem == null)
                        return;

                    var referrers = Globals.LinkDatabase.GetReferrers(mediaItem).ToList();

                    if (referrers.Any())
                    {
                        SheerResponse.Confirm($"This media item is referenced by {referrers.Count} other items.\n\nEditing the media item will change it for all the referencing items.\n\nAre you sure you want to continue?");
                        args.WaitForPostBack();
                        return;
                    }
                }

                var medItm = Client.ContentDatabase.GetItem(mediaId);

                if (medItm == null)
                    Windows.RunApplication("Media/Imager", $"id=\"{mediaId}&la={ItemLanguage}");

                var webDavEdit = "webdav:compositeedit";
                var command = CommandManager.GetCommand(webDavEdit);

                if (command == null)
                {
                    SheerResponse.Alert(Translate.Text("Edit command not found."));
                }
                else
                {
                    switch (CommandManager.QueryState(webDavEdit, medItm))
                    {
                        case CommandState.Disabled:
                        case CommandState.Hidden:
                            Windows.RunApplication("Media/Imager", "id=" + mediaId + "&la=" + ItemLanguage);
                            break;
                    }
                    command.Execute(new CommandContext(medItm));
                }
            }
        }

        protected void Update()
        {
            var src = string.Empty;
            GetSrc(out src);
            SheerResponse.SetAttribute(ID + "_image", "src", src);
            SheerResponse.SetInnerHtml(ID + "_details", GetDetails());
            SheerResponse.Eval("scContent.startValidators()");
        }

        protected void SetValue(MediaItem item)
        {
            XmlValue.SetAttribute("mediaid", item.ID.ToString());
            Value = GetMediaPath();
        }

        public override void SetValue(string value)
        {
            XmlValue = new XmlValue(value, "image");
            Value = GetMediaPath();
        }

        private void ClearImage()
        {
            if (Disabled)
                return;

            if (Value.Length > 0)
                SetModified();

            XmlValue = new XmlValue(string.Empty, "image");
            Value = string.Empty;
            AssociatedMovieValue = string.Empty;
            Update();
        }

        private void GetSrc(out string src)
        {
            src = string.Empty;
            var mediaItem = (MediaItem) GetMediaItem();

            if (mediaItem == null)
                return;

            var thumbnailOptions = MediaUrlOptions.GetThumbnailOptions(mediaItem);
            int result;
            if (!int.TryParse(mediaItem.InnerItem["Height"], out result))
                result = 128;

            thumbnailOptions.Height = Math.Min(128, result);
            thumbnailOptions.MaxWidth = 640;
            thumbnailOptions.UseDefaultIcon = true;
            src = MediaManager.GetMediaUrl(mediaItem, thumbnailOptions);
        }

        public string MovieSource
        {
            get { return GetViewStateString("MovieSource"); }
            set
            {
                var str = MainUtil.UnmapPath(value);
                if (str.EndsWith("/", StringComparison.InvariantCulture))
                    str = str.Substring(0, str.Length - 1);

                SetViewStateString("MovieSource", str);
            }
        }

        private string GetDetails()
        {
            var details = string.Empty;
            var mediaItem = (MediaItem) GetMediaItem();

            if (mediaItem != null)
            {
                var innerItem = mediaItem.InnerItem;
                var sb = new StringBuilder();
                var xmlValue = XmlValue;
                var dimensions = innerItem["Dimensions"];
                var width = HttpUtility.HtmlEncode(xmlValue.GetAttribute("width"));
                var height = HttpUtility.HtmlEncode(xmlValue.GetAttribute("height"));

                sb.Append("<div>");

                if (!string.IsNullOrEmpty(width) || !string.IsNullOrEmpty(height))
                    sb.Append(Translate.Text("Dimensions: {0} x {1} (Original: {2})", width, height, dimensions));
                else
                    sb.Append(Translate.Text("Dimensions: {0}", dimensions));

                var associatedMovie = GetAssociatedMovieItem();

                if (associatedMovie != null)
                    sb.Append(Translate.Text("           - Live Photo Movie File: {0}", associatedMovie.DisplayName ?? associatedMovie.Name));

                sb.Append("</div>");
                sb.Append("<div style=\"padding:2px 0px 0px 0px\">");

                var alt = HttpUtility.HtmlEncode(innerItem["Alt"]);
                var xmlAlt = HttpUtility.HtmlEncode(xmlValue.GetAttribute("alt"));

                if (!string.IsNullOrEmpty(xmlAlt) && !string.IsNullOrEmpty(alt))
                    sb.Append(Translate.Text("Alternate Text: \"{0}\" (Default Alternate Text: \"{1}\")", xmlAlt, alt));
                else if (!string.IsNullOrEmpty(xmlAlt))
                    sb.Append(Translate.Text("Alternate Text: \"{0}\"", xmlAlt));
                else if (!string.IsNullOrEmpty(alt))
                    sb.Append(Translate.Text("Default Alternate Text: \"{0}\"", alt));
                else
                    sb.Append(Translate.Text("Warning: Alternate Text is missing."));

                sb.Append("</div>");

                details = sb.ToString();
            }

            if (details.Length == 0)
                details = Translate.Text("This media item has no details.");

            return details;
        }

        private Item GetMediaItem()
        {
            var attribute = XmlValue.GetAttribute("mediaid");
            return attribute.Length <= 0 ? null : Client.ContentDatabase.GetItem(attribute, Language.Parse(ItemLanguage));
        }

        private Item GetAssociatedMovieItem()
        {
            var attribute = XmlValue.GetAttribute("movieid");
            return attribute.Length <= 0 ? null : Client.ContentDatabase.GetItem(attribute, Language.Parse(ItemLanguage));
        }

        private string GetMediaPath()
        {
            var mediaItem = (MediaItem) GetMediaItem();
            return mediaItem != null ? mediaItem.MediaPath : string.Empty;
        }

        private string GetMoviePath()
        {
            var mediaItem = (MediaItem) GetAssociatedMovieItem();
            return mediaItem != null ? mediaItem.MediaPath : string.Empty;
        }

        private bool IsImageMedia(TemplateItem template)
        {
            if (template.ID == TemplateIDs.VersionedImage || template.ID == TemplateIDs.UnversionedImage)
                return true;

            return template.BaseTemplates.Any(IsImageMedia);
        }
    }
}