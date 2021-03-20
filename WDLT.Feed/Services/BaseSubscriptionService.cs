using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WDLT.Feed.Database;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;
using WDLT.Feed.Properties;

namespace WDLT.Feed.Services
{
    public abstract class BaseSubscriptionService 
    {
        public abstract ESource Source { get; }

        public abstract Task<List<DBCard>> GetCardsAsync(DBSubscription subscription);
        public abstract Task<DBSubscription> CreateSubscriptionAsync(Uri uri);

        public virtual Task<bool> Init()
        {
            return Task.FromResult(true);
        }
    }
}