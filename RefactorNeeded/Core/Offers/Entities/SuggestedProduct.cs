using RefactorNeeded.Commons;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Products;

namespace RefactorNeeded.Core.Offers.Entities
{
    public class SuggestedProduct : Entity
    {
        public ProductId ProductId { get; }

        public Money Price { get; }

        public SuggestedProduct(ProductId productId, Money price)
        {
            ProductId = productId;
            Price = price;
        }
    }
}