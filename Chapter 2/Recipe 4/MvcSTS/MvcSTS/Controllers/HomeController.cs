using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Web;
using System.Globalization;

namespace MvcSTS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(WSFederationMessage message)
        {
            try
            {
                if (message.Action == WSFederationConstants.Actions.SignIn)
                {
                    // Process signin request.
                    SignInRequestMessage requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(Request.Url);
                    if (User != null && User.Identity != null && User.Identity.IsAuthenticated)
                    {
                        SecurityTokenService sts = new CustomSecurityTokenService(CustomSecurityTokenServiceConfiguration.Current);
                        SignInResponseMessage responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, User, sts);
                        FederatedPassiveSecurityTokenServiceOperations.ProcessSignInResponse(responseMessage, System.Web.HttpContext.Current.Response);
                    }
                    else
                    {
                        throw new UnauthorizedAccessException();
                    }
                }
                else if (message.Action == WSFederationConstants.Actions.SignOut)
                {
                    // Process signout request.
                    SignOutRequestMessage requestMessage = (SignOutRequestMessage)WSFederationMessage.CreateFromUri(Request.Url);
                    FederatedPassiveSecurityTokenServiceOperations.ProcessSignOutRequest(requestMessage, User, requestMessage.Reply, System.Web.HttpContext.Current.Response);
                }
                else
                {
                    throw new InvalidOperationException(
                        String.Format(CultureInfo.InvariantCulture,
                                       "The action '{0}' (Request.QueryString['{1}']) is unexpected. Expected actions are: '{2}' or '{3}'.",
                                       String.IsNullOrEmpty(message.Action) ? "<EMPTY>" : message.Action,
                                       WSFederationConstants.Parameters.Action,
                                       WSFederationConstants.Actions.SignIn,
                                       WSFederationConstants.Actions.SignOut));
                }
            }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exception)
            {
                throw new Exception("An unexpected error occurred when processing the request. See inner exception for details.", exception);
            }

            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
