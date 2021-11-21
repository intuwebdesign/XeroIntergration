using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;

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
                Scope           = "openid"
            };

            return xconfig;
        }

        public static void StoreToken(XeroToken xeroToken)
        {
            var dir         = HttpContext.Current.Server.MapPath("~/XeroToken");
            var file        = Path.Combine(dir, "xeroToken.json");
            string serilizeToken   = JsonConvert.SerializeObject(xeroToken);
            File.WriteAllText(file,serilizeToken);
        }

        public static XeroToken RetrieveToken()
        {
            var dir = HttpContext.Current.Server.MapPath("~/XeroToken");
            var file = Path.Combine(dir, "xeroToken.json");

            using (StreamReader streamReader = new StreamReader(file))
            {
                string json = streamReader.ReadToEnd();
                XeroToken token = JsonConvert.DeserializeObject<XeroToken>(json);
                return token;
            }
        }

        public class XeroToken
        {
            public string AccessToken    { get; set; }
            public DateTime ExpiresAtUtc { get; set; }
            public string IdToken        { get; set; }
            public string RefreshToken   { get; set; }
            public List<Tenant> Tenant   { get; set; }
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