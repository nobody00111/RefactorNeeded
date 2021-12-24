using System;
using RefactorNeeded.Commons;
using RefactorNeeded.Commons.ErrorHandling;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Offers.Enums;
using RefactorNeeded.Core.Offers.ValueObjects;
using RefactorNeeded.Core.Products;

namespace RefactorNeeded.Core.Offers.Entities
{
    public class OfferItem : Entity
    {
        public ProductId ProductId { get; }

        public string ProductName { get; }

        public Quantity Quantity { get; private set; }

        public Money UnitPrice { get; private set; }

        public Money UnitPriceAfterDiscount { get; private set; }

        public Discount? Discount { get; private set; }

        public bool IsManualPrice { get; private set; }

        public Money MinimalPrice { get; }

        public Money? ApprovedPrice { get; private set; }

        public Money TotalUnitPrice { get; private set; }

        public Money TotalUnitPriceAfterDiscount { get; private set; }

        public bool IsDiscountApplied => Discount != null;

        public bool IsDiscountThresholdExceeded
            => MinimalPrice > UnitPriceAfterDiscount && (ApprovedPrice == null || MinimalPrice > ApprovedPrice);

        #nullable disable
        private OfferItem()
        {
        }
        #nullable restore

        internal OfferItem(Product product, Quantity quantity, Money unitPrice, Money minimalPrice)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Quantity = quantity;
            UnitPrice = unitPrice;
            MinimalPrice = minimalPrice;

            TotalUnitPrice = new Money(0, unitPrice.Currency);
            TotalUnitPriceAfterDiscount = new Money(0, unitPrice.Currency);
            UnitPriceAfterDiscount = new Money(0, unitPrice.Currency);

            RefreshPrices();
        }

        internal void SetDiscount(Discount discount)
        {
            if (discount.Value == 0)
                Discount = null;
            else
                Discount = discount;

            RefreshPrices();
        }

        internal void SetUnitPrice(Money price)
        {
            IsManualPrice = true;
            UnitPrice = price;

            RefreshPrices();
        }

        internal void SetQuantity(Quantity quantity)
        {
            Quantity = quantity;

            RefreshPrices();
        }

        public void ApprovePrice()
        {
            ApprovedPrice = UnitPrice;
        }

        private void RefreshPrices()
        {
            if (UnitPrice == null) throw new InvalidOperationException(nameof(UnitPrice));

            if (Quantity == null) throw new InvalidOperationException(nameof(Quantity));

            TotalUnitPrice = UnitPrice.Multiply(Quantity);

            if (Discount != null)
            {
                UnitPriceAfterDiscount = CalculateUnitPriceAfterDiscount(Discount);
                TotalUnitPriceAfterDiscount = UnitPriceAfterDiscount.Multiply(Quantity);
            }
            else
            {
                UnitPriceAfterDiscount = UnitPrice;
                TotalUnitPriceAfterDiscount = TotalUnitPrice;
            }
        }

        internal Either<Success, DomainError> ValidateDiscount(Discount discount)
        {
            if (discount.Type == DiscountType.Amount && discount.Currency != UnitPrice.Currency)
                return new DomainError("offers.set_discount.wrong_currency", "Currencies do not match.");

            if (discount.Type == DiscountType.Amount && discount.Value > UnitPrice.Value)
                return new DomainError("offers.set_discount.discount_too_big",
                    "Discount value cannot be bigger than unit price.");

            return Success.Value;
        }

        internal Money CalculateUnitPriceAfterDiscount(Discount discount)
        {
            return discount.ApplyToPrice(UnitPrice);
        }
    }
}