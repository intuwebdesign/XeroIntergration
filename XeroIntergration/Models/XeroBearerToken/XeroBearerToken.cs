using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
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
        /// Retuens all the customers except those archived, to return archived change includeArchived to true
        /// </summary>
        /// <returns>ListOfCustomers</returns>
        public InvoiceCustomer GetCompanyNameAndId()
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

                AccountingApi accountingApi         = new AccountingApi();

                var xeroResponse = Task.Run(() => accountingApi.GetContactsAsync(accessToken: accessToken, xeroTenantId: tenantId, ifModifiedSince: null, where: null, order: null, iDs: null, page: 1, includeArchived: false, summaryOnly: false, searchTerm: null)).Result;

                var listOfCustomers = xeroResponse._Contacts.Select(r => new SelectListItem { Text = r.Name, Value = r.ContactID.ToString() }).ToList();

                var model = new InvoiceCustomer
                {
                    ListOfCustomers = listOfCustomers
                };

                return model;
            }
            return null;
        }

        /// <summary>
        /// Create a new invoice
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Will return the invoice number</returns>
        public string CreateNewCustomerInvoice(InvoiceCustomer model)
        {
            bool doesXeroTokenFileExist = XeroConfigurationHelper.DoesXeroTokenFileExist();
            XeroClient client;

            if (doesXeroTokenFileExist)
            {
                var token = XeroConfigurationHelper.RetrieveToken();

                if (token == null) return "No Token";

                var xeroTokenExpires = token.ExpiresAtUtc;

                if (DateTime.Now.ToUniversalTime() > xeroTokenExpires)
                {
                    client = new XeroClient(XeroConfigurationHelper.XeroConfiguration());
                    var refreshToken = token;
                    token = (XeroOAuth2Token)Task.Run(() => client.RefreshAccessTokenAsync(refreshToken)).Result;

                    XeroConfigurationHelper.StoreToken(token);
                }

                string accessToken  = token.AccessToken;
                string tenantId     = token.Tenants[0].TenantId.ToString();

                var contact = new Contact
                {
                    ContactID = Guid.Parse(model.Customers)
                };

                var line = new LineItem
                {
                    Description     = model.Description,
                    Quantity        = model.Quantity,
                    UnitAmount      = model.UnitAmount,
                    AccountCode     = model.AccountCode,
                    TaxType         = model.TaxType
                };

                var lines = new List<LineItem>
                {
                    line
                };

                var invoice = new Invoice
                {
                    Type            = Invoice.TypeEnum.ACCREC,
                    Contact         = contact,
                    Date            = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")),
                    DueDate         = Convert.ToDateTime(DateTime.Now.AddDays(14).ToString("yyyy-MM-dd")),
                    LineItems       = lines,
                    CurrencyCode    = CurrencyCode.GBP,
                    Status          = Invoice.StatusEnum.AUTHORISED
                };

                var invoiceList = new List<Invoice> { invoice };

                var invoices = new Invoices
                {
                    _Invoices = invoiceList
                };


                AccountingApi accountingApi = new AccountingApi();

                var xeroResponse = Task.Run(() => accountingApi.CreateInvoicesAsync(accessToken, tenantId, invoices)).Result;
                
                Guid invoiceId              = new Guid(xeroResponse._Invoices[0].InvoiceID.ToString());
                RequestEmpty requestEmpty   = new RequestEmpty();

                var sendEmailInvoice = Task.Run(() => accountingApi.EmailInvoiceAsync(accessToken, tenantId, invoiceId, requestEmpty)).Id;

                var returnStatus = $"Invoice Number {xeroResponse._Invoices[0].InvoiceNumber} Email Sent ID {sendEmailInvoice}";
                return returnStatus;
            }

            return "No File";
        }
    }
}