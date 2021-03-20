using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using RestSharp;
using WDLT.Clients.Base;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;


namespace WDLT.Feed.Services
{
    public class RssService : BaseSubscriptionService
    {
        public override ESource Source { get; }

        public RssService()
        {
            Source = ESource.RSS;
        }

        public override async Task<List<DBCard>> GetCardsAsync(DBSubscription subscription)
        {
            var items = new List<DBCard>();

            using (var xmlReader = XmlReader.Create(new StringReader(await BaseClient.GetStringAsync(new RestRequest(subscription.SourceId))), new XmlReaderSettings { Async = true, IgnoreComments = true, IgnoreWhitespace = true }))
            {
                XmlFeedReader feedReader;

                if (new Atom10FeedFormatter().CanRead(xmlReader))
                {
                    feedReader = new AtomFeedReader(xmlReader);
                }
                else
                {
                    feedReader = new RssFeedReader(xmlReader);
                }

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            var item =  await feedReader.ReadItem();

                            var card  = new DBCard
                            {
                                CardId = item.Id,
                                Header = subscription.Username,
                                Text = item.Title.Trim().Replace("&quot;", "\""),
                                Timestamp = item.Published,
                                OriginalUrl = item.Links.FirstOrDefault(f => string.Equals(f.RelationshipType, "alternate", StringComparison.OrdinalIgnoreCase))?.Uri.AbsoluteUri
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
            using (var xmlReader = XmlReader.Create(uri.AbsoluteUri, new XmlReaderSettings { Async = true, IgnoreComments = true, IgnoreWhitespace = true}))
            {
                XmlFeedReader feedReader;

                if (new Atom10FeedFormatter().CanRead(xmlReader))
                {
                    feedReader = new AtomFeedReader(xmlReader);
                }
                else
                {
                    feedReader = new RssFeedReader(xmlReader);
                }

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
                                    Source = ESource.RSS,
                                    IsProtected = false,
                                    SourceId = uri.AbsoluteUri,
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