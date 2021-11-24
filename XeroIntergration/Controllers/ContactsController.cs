using System.Web.Mvc;
using XeroIntergration.Models.Customer;
using XeroIntergration.Models.XeroBearerToken;

namespace XeroIntergration.Controllers
{
    public class ContactsController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            CustomerViewModel model = new CustomerViewModel();
            return View(model);
        }

        [HttpGet]
        public ActionResult ListOfCustomers()
        {
            IXeroBearerToken token = new XeroBearerToken();
            var listOfCustomer = token.GetCompanyNameAndId();
            return View("~/Views/Contacts/ListOfCustomers.cshtml", listOfCustomer);
        }

        [HttpPost]
        public ActionResult CreateCustomer(CustomerViewModel model)
        {
            IXeroBearerToken token = new XeroBearerToken();
            var xeroToken = token.CreateNewCustomer(model);

            ViewBag.Status = xeroToken ? "Customer Created Successfully" : "Failed to create customer";


            return View();
        }
    }
}