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
            
            XeroOAuth2Token token     = (XeroOAuth2Token)bearerToken.Result;
            DateTime expiryDateTime   = bearerToken.Result.ExpiresAtUtc;

            XeroConfigurationHelper.StoreToken(token);
           
            return token.AccessToken;
        }

        public bool CreateNewCustomer()
        {
            //TODO for later blog
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();
            XeroClient client;

            if (doesXeroTokenFileExist)
            {
                var token = XeroConfigurationHelper.RetrieveToken();

                if (token == null) return false;

                var xeroTokenExpires = token.ExpiresAtUtc;

                if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                {
                    client            = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                    var refreshToken  = token;
                    token             = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;
                    XeroConfigurationHelper.StoreToken(token);

                    return false;
                }
            }
            return false;
        }

        public bool CreateNewCustomerInvoice()
        {
            //TODO for later blog
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();
            XeroClient client;

            if (!doesXeroTokenFileExist) return false;

            var token = XeroConfigurationHelper.RetrieveToken();

            if (token != null)
            {
                var xeroTokenExpires = token.ExpiresAtUtc;

                if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                {
                    client           = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                    var refreshToken = token;
                    token            = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

                    XeroConfigurationHelper.StoreToken(token);

                    return false;
                }
            }

            return false;
        }
    }
}