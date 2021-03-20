using Stylet;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;

namespace WDLT.Feed.Models
{
    public class AppSourceList
    {
        public AppSourceList(ESource source)
        {
            Source = source;
            Sources = new BindableCollection<DBSubscription>();
        }

        public ESource Source { get; }
        public BindableCollection<DBSubscription> Sources { get; }
    }
}