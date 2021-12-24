using System;
using System.Globalization;
using RefactorNeeded.Commons;
using RefactorNeeded.Commons.ValueObjects;
using RefactorNeeded.Core.Offers.Enums;

namespace RefactorNeeded.Core.Offers.ValueObjects
{
    public class Discount : ValueObject
    {
        public DiscountType Type { get; }

        public decimal Value { get; }

        public Currency Currency { get; }

        public static Discount ZeroPercentageDiscount => GetPercentageDiscount(0);

        public static Discount GetZeroAmountDiscount(Currency currency)
        {
            return GetAmountDiscount(0, currency);
        }

        public static Discount GetPercentageDiscount(decimal value)
        {
            if (value < 0 || value > 100)
                throw new InvalidOperationException("Incorrect percentage discount value: " + value);

            return new Discount(DiscountType.Percentage, value, Currency.PLN);
        }

        public static Discount GetAmountDiscount(decimal value, Currency currency)
        {
            return new(DiscountType.Amount, value, currency);
        }

        public Money ApplyToPrice(Money money)
        {
            var discountAmountValue = Type switch
            {
                DiscountType.Percentage => Value / 100m * money.Value,
                DiscountType.Amount when Currency == money.Currency => Value,
                _ => throw new InvalidOperationException(
                    $"Cannot apply discount with Currency: {Currency} to price with Currency: {money.Currency}")
            };

            var priceToSubtract = new Money(discountAmountValue, money.Currency);
            return money.Subtract(priceToSubtract);
        }

        private Discount(DiscountType type, decimal value, Currency currency)
        {
            if (value < 0) RaiseError(nameof(value), value.ToString(CultureInfo.InvariantCulture));

            Type = type;
            Value = value;
            Currency = currency;
        }
    }
}