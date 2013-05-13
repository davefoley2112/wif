using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml.Linq;
using ReservationService.Entities;
using ReservationService.Client.Helpers;
using System.Collections.Specialized;
using System.Configuration;
using System.Web;

namespace ReservationService.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // First start the web project, then the client
                WebClient client = new WebClient();
                var token = RetrieveACSToken();
                client.Headers.Add("Authorization", token);
                client.Headers.Add("Content-type", "text/xml");
                var url = new Uri("http://localhost:2795/ReservationService/CreateReservation");

                var newReservation = new Reservation { GuestName = "Jack Sparrow", ReservationDate = DateTime.Now.AddDays(20) };
                var resString = EntitySerializer.GetString<Reservation>(newReservation);
                var result = client.UploadString(url, "POST", resString);
                var createdReservation = EntitySerializer.GetObject<Reservation>(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static string RetrieveACSToken()
        {
            var acsHostName = ConfigurationManager.AppSettings.Get("ACSHostName");
            var acsNamespace = ConfigurationManager.AppSettings.Get("ACSNamespace");
            var username = ConfigurationManager.AppSettings.Get("ServiceIdentityUserName");
            var password = ConfigurationManager.AppSettings.Get("ServiceIdentityCredentialPassword");
            var scope = "http://localhost:2795/ReservationService/";
            
            // request a token from ACS
            WebClient client = new WebClient();
            client.BaseAddress = string.Format("https://{0}.{1}", acsNamespace, acsHostName);
            NameValueCollection values = new NameValueCollection();
            values.Add("wrap_name", username);
            values.Add("wrap_password", password);
            values.Add("wrap_scope", scope);

            byte[] responseBytes = client.UploadValues("WRAPv0.9", "POST", values);
            string response = Encoding.UTF8.GetString(responseBytes);

            string token = response
                             .Split('&')
                             .Single(value => value.StartsWith("wrap_access_token=", StringComparison.OrdinalIgnoreCase))
                             .Split('=')[1];

            var decodedToken = string.Format("WRAP access_token=\"{0}\"", HttpUtility.UrlDecode(token));
            return decodedToken;
        }
    }
}
