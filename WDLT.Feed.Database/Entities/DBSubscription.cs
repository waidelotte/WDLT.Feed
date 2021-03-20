using System;
using System.Collections.Generic;
using WDLT.Feed.Database.Enums;

namespace WDLT.Feed.Database.Entities
{
    public class DBSubscription : NotificationEntity
    {
        public long Id { get; set; }

        public string SourceId { get; set; }
        public ESource Source { get; set; }

        private string _username;
        public string Username
        {
            get => _username;
            set => SetWithNotify(value, ref _username);
        }

        public bool IsProtected { get; set; }
        public DateTimeOffset LastTimestamp { get; set; }

        public virtual List<DBCard> Cards { get; set; }
        public virtual List<DBBlacklist> Blacklist { get; set; }
    }
}