using System;

namespace RefactorNeeded.Commons.ValueObjects
{
    public class Percent : ValueObject
    {
        public int Value { get; }

        public Percent(int value)
        {
            if (value < 0 || value > 100) throw new InvalidOperationException("Invalid percentage: " + value);

            Value = value;
        }
    }
}