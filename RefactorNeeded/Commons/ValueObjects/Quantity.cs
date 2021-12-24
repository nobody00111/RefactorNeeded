using System;

namespace RefactorNeeded.Commons.ValueObjects
{
    public class Quantity : ValueObject
    {
        public int Value { get; }

        public static readonly Quantity Single = new(1);

        public Quantity(int value)
        {
            if (value <= 0) throw new InvalidOperationException("Invalid quantity:" + value);

            Value = value;
        }

        public static Quantity operator +(Quantity quantity1, Quantity quantity2)
        {
            return new(quantity1.Value + quantity2.Value);
        }

        public static Quantity operator -(Quantity quantity1, Quantity quantity2)
        {
            return new(quantity1.Value - quantity2.Value);
        }
    }
}