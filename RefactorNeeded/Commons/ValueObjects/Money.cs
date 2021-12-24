using System;

namespace RefactorNeeded.Commons.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Value { get; }

        public Currency Currency { get; }

        public static Money Zero(Currency currency)
        {
            return new(0, currency);
        }

        public Money(decimal value, Currency currency)
        {
            if (value < 0) throw new InvalidOperationException("Incorrect price: " + value);

            Value = value;
            Currency = currency;
        }

        public Money Subtract(Money moneyToSubtract)
        {
            if (Currency != moneyToSubtract.Currency)
                throw new InvalidOperationException(
                    $"Cannot subtract money with CurrencyId: {moneyToSubtract.Currency} from money with CurrencyId: {Currency}");

            return new Money(Value - moneyToSubtract.Value, Currency);
        }

        public Money Add(Money moneyToAdd)
        {
            if (Currency != moneyToAdd.Currency)
                throw new InvalidOperationException(
                    $"Cannot add money with CurrencyId: {moneyToAdd.Currency} to money with CurrencyId: {Currency}");

            return new Money(Value + moneyToAdd.Value, Currency);
        }

        public Money Increase(Percent percent)
        {
            return Add(Multiply(percent));
        }

        public Money Decrease(Percent percent)
        {
            return Subtract(Multiply(percent));
        }

        public Money Multiply(decimal multiplier)
        {
            if (multiplier <= 0) throw new InvalidCastException("Incorrect multiplier:" + multiplier);

            return new Money(Value * multiplier, Currency);
        }

        public Money Multiply(Quantity quantity)
        {
            return Multiply(quantity.Value);
        }

        public Money Multiply(Percent percent)
        {
            return Multiply(percent.Value / 100m);
        }

        public static bool operator <(Money money1, Money money2)
        {
            if (money1.Currency != money2.Currency) throw new InvalidOperationException("Currencies do not match.");

            return money1.Value < money2.Value;
        }

        public static bool operator >(Money money1, Money money2)
        {
            return !(money1 < money2) && money1 != money2;
        }
    }
}