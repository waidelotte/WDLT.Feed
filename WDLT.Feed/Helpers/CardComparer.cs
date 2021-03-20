using System.Collections.Generic;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Models;

namespace WDLT.Feed.Helpers
{
    internal class CardComparer : IComparer<DBCard>
    {
        public int Compare(DBCard x, DBCard y)
        {
            switch (x.Timestamp.CompareTo(y.Timestamp))
            {
                case 1:
                    return -1;
                case -1:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}