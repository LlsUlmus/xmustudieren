using Microsoft.AspNetCore.Mvc;

namespace Ricebird.Cxw.Controllers
{
    public class HomeController : Controller
    {
        [Route("/authen/login")]
        public ActionResult ManageLogin()
        {
            return File("~/manage/index.html", "text/html");
        }

#pragma warning disable ASP0018 // Unused route parameter
        [Route("/manage/{**catch}")]
#pragma warning restore ASP0018 // Unused route parameter
        public ActionResult ManageHome()
        {
            return File("~/manage/index.html", "text/html");
        }

        [Route("~/mobile/{**catch}")]
        public ActionResult ToMobile()
        {
            return File("/mobile/index.html", "text/html");
        }
    }
}
