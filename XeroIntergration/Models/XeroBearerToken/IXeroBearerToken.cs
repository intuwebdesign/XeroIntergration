using XeroIntergration.Models.Customer;

namespace XeroIntergration.Models.XeroBearerToken
{
    public interface IXeroBearerToken
    {
        string LoginUrl();
        string BearerToken(string code);
        string CreateNewCustomerInvoice(InvoiceCustomer model);
        bool CreateNewCustomer(CustomerViewModel model);
        InvoiceCustomer GetCompanyNameAndId();
    }
}