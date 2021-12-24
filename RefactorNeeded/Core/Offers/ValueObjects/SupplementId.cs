using System;
using RefactorNeeded.Commons;

namespace RefactorNeeded.Core.Offers.ValueObjects
{
    public class SupplementId : TypedId
    {
        public SupplementId(Guid value) : base(value)
        {
        }
    }
}