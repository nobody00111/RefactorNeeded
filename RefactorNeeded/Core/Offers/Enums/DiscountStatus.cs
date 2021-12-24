namespace RefactorNeeded.Core.Offers.Enums
{
    public enum DiscountStatus
    {
        ThresholdNotExceeded = 0,
        ThresholdExceeded = 1,
        SentForApproval = 2,
        Approved = 3,
        Rejected = 4
    }
}