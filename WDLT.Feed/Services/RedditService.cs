using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using RestSharp;
using WDLT.Clients.Base;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;
using WDLT.Feed.Helpers;


namespace WDLT.Feed.Services
{
    public class RedditService : BaseSubscriptionService
    {
        public override ESource Source { get; }

        public RedditService()
        {
            Source = ESource.Reddit;
        }

        public override async Task<List<DBCard>> GetCardsAsync(DBSubscription subscription)
        {
            var items = new List<DBCard>();

            using (var xmlReader = XmlReader.Create(new StringReader(await BaseClient.GetStringAsync(new RestRequest($"https://www.reddit.com{subscription.SourceId}"))), new XmlReaderSettings { Async = true }))
            {
                var feedReader = new AtomFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            var item = await feedReader.ReadItem();

                            var card = new DBCard
                            {
                                CardId = item.Id,
                                Text = item.Title,
                                Header = item.Categories.First().Label,
                                Timestamp = item.LastUpdated,
                                OriginalUrl = item.Links.First().Uri.AbsoluteUri
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
            var url = uri.AbsoluteUri.Contains(".rss") ? uri.AbsoluteUri : $"{uri.AbsoluteUri}.rss";

            using (var xmlReader = XmlReader.Create(url, new XmlReaderSettings { Async = true }))
            {
                var feedReader = new AtomFeedReader(xmlReader);

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Content:
                            var content = await feedReader.ReadContent();
                            if (string.Equals(content.Name, "id", StringComparison.OrdinalIgnoreCase))
                            {
                                return new DBSubscription
                                {
                                    Source = ESource.Reddit,
                                    IsProtected = false,
                                    SourceId = content.Value,
                                    Username = content.Value.Replace(".rss", string.Empty)
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