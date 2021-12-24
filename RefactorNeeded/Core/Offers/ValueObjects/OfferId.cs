using System;
using RefactorNeeded.Commons;

namespace RefactorNeeded.Core.Offers.ValueObjects
{
    public class OfferId : TypedId
    {
        public OfferId(Guid value) : base(value)
        {
        }
    }
}