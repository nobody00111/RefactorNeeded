using System;

namespace RefactorNeeded.Commons
{
    public abstract class TypedId : ValueObject
    {
        public Guid Value { get; }

        public TypedId(Guid value)
        {
            if (value == Guid.Empty) throw new InvalidOperationException("Empty guid as identifier for " + GetType());

            Value = value;
        }
    }
}