using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.PriceLists;

namespace RefactorNeeded.Core.Products
{
    public class PriceListPrice
    {
        public PriceListId PriceListId { get; }
        public Money Price { get; }

        public PriceListPrice(PriceListId priceListId, Money price)
        {
            PriceListId = priceListId;
            Price = price;
        }
    }
}