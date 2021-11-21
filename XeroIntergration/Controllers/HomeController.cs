using System.Web.Mvc;
using XeroIntergration.Models.XeroBearerToken;

namespace XeroIntergration.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IXeroBearerToken token  = new XeroBearerToken();
            var loginUrl     = token.LoginUrl();

            ViewBag.AuthUrl        = loginUrl;
            return View();
        }

        public ActionResult Authorize(string code)
        {
            IXeroBearerToken token  = new XeroBearerToken();
            var xeroToken    = token.BearerToken(code);
            ViewBag.Token          = xeroToken;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}