using RefactorNeeded.Commons.ValueObjects;

namespace RefactorNeeded.Core.PriceLists
{
    public class PriceList
    {
        public PriceListId Id { get; }

        public Currency Currency { get; }

        public PriceList(PriceListId id, Currency currency)
        {
            Id = id;
            Currency = currency;
        }
    }
}