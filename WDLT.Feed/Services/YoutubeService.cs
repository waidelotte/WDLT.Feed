using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using RestSharp;
using WDLT.Clients.Base;
using WDLT.Clients.Youtube;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;
using WDLT.Feed.Helpers;
using WDLT.Feed.Properties;


namespace WDLT.Feed.Services
{
    public class YoutubeService : BaseSubscriptionService
    {
        public override ESource Source { get; }

        private readonly YoutubeClient _client;

        public YoutubeService()
        {
            Source = ESource.Youtube;
            _client = new YoutubeClient(Settings.Default.UserAgent);
        }

        public override async Task<List<DBCard>> GetCardsAsync(DBSubscription subscription)
        {
            var items = new List<DBCard>();

            using (var xmlReader = XmlReader.Create(new StringReader(await BaseClient.GetStringAsync(new RestRequest($"https://www.youtube.com/feeds/videos.xml?channel_id={subscription.SourceId}"))), new XmlReaderSettings { Async = true }))
            {
                var feedReader = new AtomFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            var item = await feedReader.ReadItem();

                            var id = item.Id.Split(":").Last();

                            var card = new DBCard
                            {
                                CardId = item.Id,
                                Text = item.Title,
                                Header = item.Contributors.FirstOrDefault()?.Name,
                                Timestamp = item.Published,
                                OriginalUrl = $"https://www.youtube.com/embed/{id}?autoplay=1&modestbranding=0&rel=0"
                            };

                            items.Add(card);
                            break;
                    }
                }
            }

            return items;
        }

        public override async Task<DBSubscription> CreateSubscriptionAsync(Uri uri)
        {
            var id = uri.Segments.ElementAtOrDefault(2);
            if (id == null) return null;

            if (uri.Segments.ElementAt(1) == "c/" || uri.Segments.ElementAt(1) == "user/")
            {
               var channelId = await PollyHelper.WebFallbackAsync(() => _client.ChannelId(uri.AbsoluteUri));
               if (channelId == null) return null;

               id = channelId;
            }

            using (var xmlReader = XmlReader.Create($"https://www.youtube.com/feeds/videos.xml?channel_id={id}", new XmlReaderSettings { Async = true }))
            {
                var feedReader = new AtomFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Content:
                            var content = await feedReader.ReadContent();
                            if (string.Equals(content.Name, "title", StringComparison.OrdinalIgnoreCase))
                            {
                                return new DBSubscription
                                {
                                    Source = ESource.Youtube,
                                    IsProtected = false,
                                    SourceId = id,
                                    Username = content.Value
                                };
                            }
                            break;
                    }
                }
            }

            return null;
        }
    }
}