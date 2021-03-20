using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;

namespace WDLT.Feed.Models
{
    public class ExportSubscription
    {
        public ExportSubscription() {}

        public ExportSubscription(DBSubscription subscription)
        {
            Source = subscription.Source;
            SourceId = subscription.SourceId;
            Username = subscription.Username;
            IsProtected = subscription.IsProtected;
        }

        public ESource Source { get; set; }
        public string SourceId { get; set; }
        public string Username { get; set; }
        public bool IsProtected { get; set; }
    }
}