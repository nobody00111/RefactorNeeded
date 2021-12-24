using System.Collections.Generic;
using System.Linq;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Offers.Config;
using RefactorNeeded.Core.PriceLists;

namespace RefactorNeeded.Core.Products
{
    public class Product
    {
        public ProductId Id { get; }

        public string Name { get; }

        public Money BasePrice { get; }

        public IReadOnlyCollection<PriceListPrice> Prices { get; }

        public MinimalPrice GetProductMinimalPrice(DiscountThresholdConfig discountThresholdConfig)
        {
            return new(BasePrice.Increase(new Percent(discountThresholdConfig.DiscountThresholdValue)));
        }

        public Product(ProductId id, string name, Money basePrice, List<PriceListPrice> prices)
        {
            Id = id;
            Name = name;
            BasePrice = basePrice;
            Prices = prices.AsReadOnly();
        }

        public PriceListPrice? GetPrice(PriceListId priceListId)
        {
            return Prices.SingleOrDefault(x => x.PriceListId == priceListId);
        }
    }
}