using WDLT.Feed.Database.Enums;

namespace WDLT.Feed.Helpers
{
    public static class Extensions
    {
        public static ESource? HostToSource(this string host)
        {
            switch (host.ToLower())
            {
                case "twitter.com":
                    return ESource.Twitter;
                case "www.youtube.com":
                case "youtube.com":
                    return ESource.Youtube;
                case "www.reddit.com":
                case "reddit.com":
                    return ESource.Reddit;
                default:
                    return ESource.RSS;
            }
        }
    }
}