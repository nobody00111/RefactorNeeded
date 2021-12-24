using System;

namespace RefactorNeeded.Commons.ValueObjects
{
    public class CurrencyExchangeRate
    {
        public Currency SourceCurrency { get; }

        public Currency TargetCurrency { get; }

        public decimal Value { get; }

        public CurrencyExchangeRate(Currency sourceCurrency, Currency targetCurrency, decimal value)
        {
            if (value <= 0) throw new ArgumentException("Invalid exchange rate:" + value);

            Value = value;
        }

        public Money Convert(Money money)
        {
            if (money.Currency != SourceCurrency)
                throw new InvalidOperationException(
                    $"Expected currency: {SourceCurrency}. Given: {money.Currency}.");

            return money.Multiply(Value);
        }

        public Money ConvertBack(Money money)
        {
            if (money.Currency != TargetCurrency)
                throw new InvalidOperationException(
                    $"Expected currency: {TargetCurrency}. Given: {money.Currency}.");

            return money.Multiply(1m / Value);
        }
    }
}