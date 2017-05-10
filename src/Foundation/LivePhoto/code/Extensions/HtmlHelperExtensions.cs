using System.Web.Mvc;
using Glass.Mapper.Sc;
using Glass.Mapper.Sc.IoC;
using Paragon.Foundation.LivePhoto.Mvc;

namespace Paragon.Foundation.LivePhoto
{
    public static class HtmlHelperExtensions
    {
        public static LivePhotoMvc<T> LivePhoto<T>(this HtmlHelper<T> htmlHelper)
        {
            return new LivePhotoMvc<T>(new GlassHtml(SitecoreContextFactory.Default.GetSitecoreContext()), 
                htmlHelper.ViewContext.Writer, htmlHelper.ViewData.Model);
        }
    }
}