using RefactorNeeded.Commons;

namespace RefactorNeeded.Core.Offers.ValueObjects
{
    public class OfferNumber : ValueObject
    {
        public string Value { get; }

        public OfferNumber(string value)
        {
            Value = value;
        }
    }
}