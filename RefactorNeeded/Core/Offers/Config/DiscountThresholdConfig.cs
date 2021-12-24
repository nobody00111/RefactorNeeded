namespace RefactorNeeded.Core.Offers.Config
{
    public class DiscountThresholdConfig
    {
        public static readonly DiscountThresholdConfig Default = new(20);

        public int DiscountThresholdValue { get; }

        public DiscountThresholdConfig(int discountThresholdValue)
        {
            DiscountThresholdValue = discountThresholdValue;
        }
    }
}