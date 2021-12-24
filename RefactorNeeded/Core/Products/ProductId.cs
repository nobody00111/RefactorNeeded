using System;
using RefactorNeeded.Commons;

namespace RefactorNeeded.Core.Products
{
    public class ProductId : TypedId
    {
        public ProductId(Guid value) : base(value)
        {
        }
    }
}