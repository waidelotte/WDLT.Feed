using WDLT.Feed.Database.Entities;
using WDLT.Feed.Enums;

namespace WDLT.Feed.Events
{
    public class NewSubscriptionEvent
    {
        public NewSubscriptionEvent(DBSubscription subscription)
        {
            Subscription = subscription;
        }

        public DBSubscription Subscription { get; }
    }
}