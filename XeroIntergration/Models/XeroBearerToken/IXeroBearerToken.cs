using System.Collections.Generic;
using XeroIntergration.Models.Customer;

namespace XeroIntergration.Models.XeroBearerToken
{
    public interface IXeroBearerToken
    {
        string LoginUrl();
        string BearerToken(string code);
        bool CreateNewCustomerInvoice();
        bool CreateNewCustomer(CustomerViewModel model);
        List<ListOfCustomers> GetCompanyNameAndId();
    }
}