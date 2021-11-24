using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace XeroIntergration.Models.Customer
{
    public class InvoiceCustomer
    {
        public List<SelectListItem> ListOfCustomers { get; set; }
        [Display(Name = "List Of Customers")]
        public string Customers                     { get; set; }
        [Display(Name = "Description")]
        public string Description                   { get; set; }
        [Display(Name = "Quantitys")]
        public int Quantity                         { get; set; }
        [Display(Name = "Price Per Unit")]
        public decimal UnitAmount                   { get; set; }
        [Display(Name = "Account Code")]
        public string AccountCode                   { get; set; }
        [Display(Name = "Tax Type")]
        public string TaxType                       { get; set; }
    }
    public class ListOfCustomers
    {
        public ListOfCustomers(string customerName, string customerId)
        {
            CustomerName    = customerName;
            CustomerId      = customerId;
        }

        public string CustomerName  { get; }
        public string CustomerId    { get; }
    }
}