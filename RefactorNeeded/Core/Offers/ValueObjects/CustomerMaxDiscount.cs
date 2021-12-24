using RefactorNeeded.Commons;
using RefactorNeeded.Commons.ValueObjects;

namespace RefactorNeeded.Core.Offers.ValueObjects
{
    public class CustomerMaxDiscount : ValueObject
    {
        public int Value { get; }

        public static readonly CustomerMaxDiscount Zero = new(0);

        public static readonly CustomerMaxDiscount Max = new(100);

        public CustomerMaxDiscount(int value)
        {
            Value = value;
        }

        public bool IsExceeded(Money priceBeforeDiscount, Money priceAfterDiscount)
        {
            return priceAfterDiscount < priceBeforeDiscount.Decrease(new Percent(Value));
        }
    }
}