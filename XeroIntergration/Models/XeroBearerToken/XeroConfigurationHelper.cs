using System;
using System.Configuration;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;

namespace XeroIntergration.Models.XeroBearerToken
{
    public static class XeroConfigurationHelper
    {
        public static XeroConfiguration XeroConfiguration()
        {
            string clientId         = ConfigurationManager.AppSettings["XeroClientId"];
            string clientSecret     = ConfigurationManager.AppSettings["XeroClientSecret"];
            string baseUrl          = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);

            XeroConfiguration xconfig = new XeroConfiguration
            {
                ClientId        = clientId,
                ClientSecret    = clientSecret,
                CallbackUri     = new Uri($"{baseUrl}/home/authorize"),
                Scope           = "openid offline_access"
            };

            return xconfig;
        }

        public static void StoreToken(XeroOAuth2Token xeroToken)
        {
            var dir         = HttpContext.Current.Server.MapPath("~/XeroToken");
            var file        = Path.Combine(dir, "xeroToken.json");
            string serilizeToken   = JsonConvert.SerializeObject(xeroToken);
            File.WriteAllText(file,serilizeToken);
        }

        public static XeroOAuth2Token RetrieveToken()
        {
            var dir = HttpContext.Current.Server.MapPath("~/XeroToken");
            var file = Path.Combine(dir, "xeroToken.json");

            using (StreamReader streamReader = new StreamReader(file))
            {
                string json = streamReader.ReadToEnd();
                var token = JsonConvert.DeserializeObject<XeroOAuth2Token>(json);
                return token;
            }
        }

        public static bool DoesXeroTokenFileExist()
        {
            var dir = HttpContext.Current.Server.MapPath("~/XeroToken");
            var file = Path.Combine(dir, "xeroToken.json");

            bool fileExists = File.Exists(file);

            return fileExists;
        }
    }
}