using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Microsoft.IdentityModel.Claims;

namespace WindowsToClaimsAuth.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";
            var principal = Thread.CurrentPrincipal;
            var identity = principal.Identity as IClaimsIdentity;
            var claims = identity.Claims;
            return View(claims);
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
