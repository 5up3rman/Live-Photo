using System.Web.Mvc;
using Glass.Mapper.Sc.Web.Mvc;
using Paragon.Foundation.LivePhoto.Models;
using Paragon.Demo.LivePhoto.Areas.LivePhoto.ViewModels;
using Paragon.Foundation.Models;

namespace Paragon.Demo.LivePhoto.Areas.LivePhoto.Controllers
{
    public class LivePhotoController : GlassController
    {
        public ActionResult Demo()
        {
            var livePhoto = new LivePhotoViewModel
            {
                LivePhoto = SitecoreContext.GetCurrentItem<ILivePhotoDemo>(inferType:true)
            };
            
            return View("/Areas/LivePhoto/Views/Demo.cshtml", livePhoto);
        }
    }
}