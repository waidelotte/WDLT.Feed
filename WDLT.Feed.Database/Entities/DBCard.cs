using System;

namespace WDLT.Feed.Database.Entities
{
    public class DBCard : NotificationEntity
    {
        public  long Id { get; set; }
        public string CardId { get; set; }
        public string Header { get; set; }
        public string Text { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool HasImage { get; set; }
        public bool HasVideo { get; set; }
        public bool HasRepost { get; set; }
        public string OriginalUrl { get; set; }

        private bool _isViewed;
        public bool IsViewed
        {
            get => _isViewed;
            set => SetWithNotify(value, ref _isViewed);
        }

        private bool _isBookmark;
        public bool IsBookmark
        {
            get => _isBookmark;
            set => SetWithNotify(value, ref _isBookmark);
        }

        private bool _isHidden;
        public bool IsHidden
        {
            get => _isHidden;
            set => SetWithNotify(value, ref _isHidden);
        }

        public long SubscriptionId { get; set; }
        public virtual DBSubscription Subscription { get; set; }
    }
}