using System.Configuration;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ReservationService.Web.Security
{
    public class ACSAuthorizationManager : ServiceAuthorizationManager
    {
        string serviceNamespace = ConfigurationManager.AppSettings.Get("ACSNamespace");
        string acsHostName = ConfigurationManager.AppSettings.Get("ACSHostName");
        string trustedTokenPolicyKey = ConfigurationManager.AppSettings.Get("IssuerSigningKey");
        string trustedAudience = "http://localhost:2795/ReservationService/";

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            string headerValue = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Authorization];

            // check that a value is there
            if (string.IsNullOrEmpty(headerValue))
            {
                CreateUnauthorizedResponse();
                return false;
            }
            // check that it starts with 'WRAP'
            if (!headerValue.StartsWith("WRAP "))
            {
                CreateUnauthorizedResponse();
                return false;
            }
            string[] nameValuePair = headerValue.Substring("WRAP ".Length).Split(new char[] { '=' }, 2);

            if (nameValuePair.Length != 2 ||
                nameValuePair[0] != "access_token" ||
                !nameValuePair[1].StartsWith("\"") ||
                !nameValuePair[1].EndsWith("\""))
            {
                CreateUnauthorizedResponse();
                return false;
            }

            // trim off the leading and trailing double-quotes
            string token = nameValuePair[1].Substring(1, nameValuePair[1].Length - 2);

            // create a token validator
            TokenValidator validator = new TokenValidator(
                this.acsHostName,
                this.serviceNamespace,
                this.trustedAudience,
                this.trustedTokenPolicyKey);

            // validate the token
            if (!validator.Validate(token))
            {
                CreateUnauthorizedResponse();
                return false;
            }
            return true;

        }

        public void CreateUnauthorizedResponse()
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Unauthorized;
            WebOperationContext.Current.OutgoingResponse.StatusDescription = "Unauthorized";
        }
    }
}