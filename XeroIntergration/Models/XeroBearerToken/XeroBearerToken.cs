using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using XeroIntergration.Models.Customer;

namespace XeroIntergration.Models.XeroBearerToken
{
    public class XeroBearerToken : IXeroBearerToken
    {
        /// <summary>
        /// Login to Xero
        /// </summary>
        /// <returns>Returns Xero login url</returns>
        public string LoginUrl()
        {
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();

            XeroClient client;
            string authorizeUrl;

            if (doesXeroTokenFileExist)
            {
                var token = XeroConfigurationHelper.RetrieveToken();

                if (token != null)
                {
                    var xeroTokenExpires = token.ExpiresAtUtc;

                    if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                    {
                        client = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                        authorizeUrl = client.BuildLoginUri();

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
            var client                      = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
            var bearerToken  = Task.Run(() => client.RequestAccessTokenAsync(code));

            XeroOAuth2Token token   = (XeroOAuth2Token)bearerToken.Result;

            var refreshToken = token;
            token            = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

            XeroConfigurationHelper.StoreToken(token);

            return token.AccessToken;
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Return true is success or false if fail</returns>
        public bool CreateNewCustomer(CustomerViewModel model)
        {
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();
            XeroClient client;

            if (doesXeroTokenFileExist)
            {
                try
                {
                    var token = XeroConfigurationHelper.RetrieveToken();

                    if (token == null) return false;

                    var xeroTokenExpires = token.ExpiresAtUtc;

                    if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                    {
                        client              = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                        var refreshToken    = token;
                        token               = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

                        XeroConfigurationHelper.StoreToken(token);
                    }

                    Guid customerId = Guid.NewGuid();

                    Address address = new Address
                    {
                        AddressLine1        = model.CompanyAddress,
                        City                = model.CompanyCity,
                        Region              = model.CompanyCounty,
                        PostalCode          = model.CompanyPostCode,
                        Country             = model.CompanyCountry,
                        AddressType         = Address.AddressTypeEnum.POBOX
                    };

                    List<Address> customerAddress = new List<Address> { address };

                    Phone phone = new Phone
                    {
                        PhoneCountryCode    = model.CompanyPhoneCountryCode,
                        PhoneAreaCode       = model.CompanyPhoneAreaCode,
                        PhoneNumber         = model.CompanyPhoneNumber,
                        PhoneType           = Phone.PhoneTypeEnum.DEFAULT
                    };

                    Phone fax = new Phone
                    {
                        PhoneCountryCode    = model.CompanyPhoneCountryCode,
                        PhoneAreaCode       = model.CompanyPhoneAreaCode,
                        PhoneNumber         = model.CompanyFaxNumber,
                        PhoneType           = Phone.PhoneTypeEnum.FAX
                    };

                    Phone mobile = new Phone
                    {
                        PhoneCountryCode    = model.CompanyPhoneCountryCode,
                        PhoneAreaCode       = model.CompanyPhoneAreaCode,
                        PhoneNumber         = model.CompanyMobileNumber,
                        PhoneType           = Phone.PhoneTypeEnum.MOBILE
                    };

                    List<Phone> phones = new List<Phone> { phone, fax, mobile };

                    var newCustomer = new Contact
                    {
                        ContactID       = customerId,
                        AccountNumber   = customerId.ToString(),
                        FirstName       = model.EmployeeName,
                        Name            = model.CompanyName,
                        EmailAddress    = model.EmployeeEmail,
                        Addresses       = customerAddress,
                        Phones          = phones,
                        IsCustomer      = true
                    };

                    var insertCustomer = new Contacts
                    {
                        _Contacts = new List<Contact>
                    {
                        newCustomer
                    }
                    };


                    var accountingApi = new AccountingApi();
                    var xeroResponse = Task.Run(() => accountingApi.CreateContactsAsync(token.AccessToken, token.Tenants[0].TenantId.ToString(), insertCustomer)).Result;

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
            return false;
        }

        /// <summary>
        /// Retuens all the customers except those archived, to do that change includeArchived to true
        /// </summary>
        /// <returns>ListOfCustomers</returns>
        public List<ListOfCustomers> GetCompanyNameAndId()
        {
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();
            XeroClient client;

            if (doesXeroTokenFileExist)
            {
                var token = XeroConfigurationHelper.RetrieveToken();

                if (token == null) return null;

                var xeroTokenExpires = token.ExpiresAtUtc;

                if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                {
                    client              = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                    var refreshToken    = token;
                    token               = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

                    XeroConfigurationHelper.StoreToken(token);
                }

                string accessToken  = token.AccessToken;
                string tenantId     = token.Tenants[0].TenantId.ToString();

                List<ListOfCustomers> customerList  = new List<ListOfCustomers>();
                AccountingApi accountingApi         = new AccountingApi();

                var xeroResponse = Task.Run(() => accountingApi.GetContactsAsync(accessToken: accessToken, xeroTenantId: tenantId, ifModifiedSince: null, where: null, order: null, iDs: null, page: 1, includeArchived: false, summaryOnly: false, searchTerm: null)).Result;

                foreach (var customer in xeroResponse._Contacts)
                {
                    customerList.Add(new ListOfCustomers(customer.Name, customer.ContactID.ToString()));
                }

                return customerList;
            }
            return null;
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
                    client = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                    var refreshToken = token;
                    token = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

                    XeroConfigurationHelper.StoreToken(token);

                    return false;
                }
            }

            return false;
        }
    }
}