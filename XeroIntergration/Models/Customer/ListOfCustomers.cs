namespace XeroIntergration.Models.Customer
{
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