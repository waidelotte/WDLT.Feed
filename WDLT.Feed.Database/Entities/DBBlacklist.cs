namespace WDLT.Feed.Database.Entities
{
    public class DBBlacklist
    {
        public long Id { get; set; }
        public string Word { get; set; }

        public long SubscriptionId { get; set; }
        public virtual DBSubscription Subscription { get; set; }
    }
}