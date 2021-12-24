using System;
using RefactorNeeded.Commons;

namespace RefactorNeeded.Core.Customers
{
    public class CustomerId : TypedId
    {
        public CustomerId(Guid value) : base(value)
        {
        }
    }
}