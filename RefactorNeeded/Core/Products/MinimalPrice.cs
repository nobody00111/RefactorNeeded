using RefactorNeeded.Commons.ValueObjects;

namespace RefactorNeeded.Core.Products
{
    public class MinimalPrice
    {
        public Money Value { get; }

        public MinimalPrice(Money value)
        {
            Value = value;
        }
    }
}