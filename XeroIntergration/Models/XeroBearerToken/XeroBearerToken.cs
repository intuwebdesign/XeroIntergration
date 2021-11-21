using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Token;

namespace XeroIntergration.Models.XeroBearerToken
{
    public class XeroBearerToken : IXeroBearerToken
    {
        public string LoginUrl()
        {
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();

            XeroClient client;
            string authorizeUrl;

            if (doesXeroTokenFileExist)
            {
                var token = XeroConfigurationHelper.RetrieveToken();

                if (token != null )
                {
                    //var xeroToken = token.Token;
                    var xeroTokenExpires = token.ExpiresAtUtc;

                    if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                    {
                        client          = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                        authorizeUrl    = client.BuildLoginUri();

                        return authorizeUrl;
                    }
                }
            }

            client = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
            authorizeUrl = client.BuildLoginUri();
            return authorizeUrl;

        }

        public string BearerToken(string code)
        {
            var client          = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
            var bearerToken     = Task.Run(()=>client.RequestAccessTokenAsync(code));
            
            IXeroToken token            = bearerToken.Result;
            DateTime expiryDateTime   = bearerToken.Result.ExpiresAtUtc;

            XeroConfigurationHelper.XeroToken xeroToken = new XeroConfigurationHelper.XeroToken
            {
                AccessToken     = token.AccessToken,
                ExpiresAtUtc    = expiryDateTime,
                IdToken         = token.IdToken,
                RefreshToken    = token.RefreshToken,
                Tenant          = token.Tenants
            };

            XeroConfigurationHelper.StoreToken(xeroToken);
           
            return token.AccessToken;
        }
    }
}