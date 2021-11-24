using System.Web.Mvc;
using XeroIntergration.Models.Customer;
using XeroIntergration.Models.XeroBearerToken;

namespace XeroIntergration.Controllers
{
    public class InvoiceController : Controller
    {
        [HttpPost]
        public ActionResult Invoice(InvoiceCustomer model)
        {
            IXeroBearerToken token = new XeroBearerToken();
            var invoice = token.CreateNewCustomerInvoice(model);

            TempData["Status"] = invoice;

            return Redirect("https://localhost:44351/Contacts/ListOfCustomers");
        }
    }
}