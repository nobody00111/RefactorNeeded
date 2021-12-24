using System;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Offers.ValueObjects;
using RefactorNeeded.Core.Products;

namespace RefactorNeeded.Core.Offers.Entities
{
    public class Supplement
    {
        public SupplementId Id { get; }

        public string Name { get; }

        public ProductId ProductId { get; }

        public Quantity Quantity { get; private set; }

        public bool IsQuantityFixed { get; }

        public Money UnitPrice { get; }

        public Money TotalPrice => UnitPrice.Multiply(Quantity);

        #nullable disable
        private Supplement()
        {
        }
        #nullable restore

        public Supplement(OfferItem offerItem, string name, Quantity? quantity, Money unitPrice)
        {
            Id = new SupplementId(Guid.NewGuid());
            ProductId = offerItem.ProductId;
            Name = name;
            UnitPrice = unitPrice;

            if (quantity != null)
            {
                IsQuantityFixed = true;
                Quantity = quantity;
            }
            else
            {
                Quantity = offerItem.Quantity;
            }
        }

        internal void ChangeQuantity(Quantity quantity)
        {
            Quantity = quantity;
        }
    }
}