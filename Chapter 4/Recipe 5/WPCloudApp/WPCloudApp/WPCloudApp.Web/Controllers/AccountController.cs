namespace WPCloudApp.Web.Controllers
{
    using System;
    using System.Globalization;
    using System.Web.Mvc;
    using System.Web.Security;
    using WPCloudApp.Web.Infrastructure;
    using WPCloudApp.Web.Models;
    using WPCloudApp.Web.UserAccountWrappers;

    [HandleError]
    public class AccountController : Controller
    {
        private readonly IUserPrivilegesRepository userPrivilegesRepository;

        public AccountController()
            : this(new UserTablesServiceContext())
        {
        }

        [CLSCompliant(false)]
        public AccountController(IUserPrivilegesRepository userPrivilegesRepository)
        {
            this.userPrivilegesRepository = userPrivilegesRepository;
        }

        public ActionResult Unauthorized()
        {
            return this.View();
        }

        public ActionResult LogOn()
        {
            return this.View(new LogOnModel());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "Needs to take same parameter type as Controller.Redirect()")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "The user name or password provided is incorrect.");
                }
            }

            return View(model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }


    }
}