using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Offers.ValueObjects;
using RefactorNeeded.Core.PriceLists;

namespace RefactorNeeded.Core.Customers
{
    public class Customer
    {
        public CustomerId Id { get; }

        public PriceListId PriceListId { get; }

        public Currency Currency { get; }

        public CustomerMaxDiscount MaxDiscount { get; }

        public Customer(CustomerId id, PriceListId priceListId, Currency currency, CustomerMaxDiscount maxDiscount)
        {
            Id = id;
            PriceListId = priceListId;
            Currency = currency;
            MaxDiscount = maxDiscount;
        }
    }
}