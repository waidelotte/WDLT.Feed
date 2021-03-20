using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WDLT.Clients.Twitter;
using WDLT.Clients.Twitter.Enums;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Database.Enums;
using WDLT.Feed.Helpers;
using WDLT.Feed.Properties;

namespace WDLT.Feed.Services
{
    public class TwitterService : BaseSubscriptionService
    {
        public override ESource Source { get; }

        private readonly TwitterClient _client;
        private DateTimeOffset _lastRequest;

        public TwitterService()
        {
            Source = ESource.Twitter;
            _client = new TwitterClient(Settings.Default.UserAgent);
            _lastRequest = DateTimeOffset.MinValue;
        }

        public override Task<bool> Init()
        {
            return UpdateToken(true);
        }

        public override async Task<DBSubscription> CreateSubscriptionAsync(Uri uri)
        {
            var username = uri.Segments[1];

            await UpdateToken();
            var user = await PollyHelper.WebFallbackAsync(() => _client.UserByScreenName(username));

            if(user != null) _lastRequest = DateTimeOffset.Now;
            if (user?.Data.User == null) return null;

            return new DBSubscription
                {
                    SourceId = user.Data.User.RestId.ToString(),
                    Username = user.Data.User.Legacy.ScreenName,
                    IsProtected = user.Data.User.Legacy.Protected,
                    Source = ESource.Twitter
                };
        }

        public override async Task<List<DBCard>> GetCardsAsync(DBSubscription subscription)
        {
            await UpdateToken();
            var timeline = await PollyHelper.WebFallbackAsync(() => _client.UserTweetsAsync(long.Parse(subscription.SourceId), Settings.Default.TwitterMaxLoadPerUser));
            if (timeline != null) _lastRequest = DateTimeOffset.Now;

            return timeline?.GlobalObjects.Tweets.Values
                .Where(w => w.UserId.ToString() == subscription.SourceId).Select(s =>
                {
                    var media = s.ExtendedEntities?.Media?.FirstOrDefault();

                    return new DBCard
                    {
                        Timestamp = s.CreatedAt,
                        Text = TrimText(s.Text),
                        CardId = s.Id.ToString(),
                        Header = $"@{subscription.Username}",
                        HasImage = media?.Type == ETwitterMediaType.Photo || media?.Type == ETwitterMediaType.Gif,
                        HasVideo = media?.Type == ETwitterMediaType.Video,
                        HasRepost = s.QuotedId != null,
                        OriginalUrl = $"https://twitter.com/{subscription.Username}/status/{s.Id}",
                    
                    };
                }).ToList();
        }

        private string TrimText(string text)
        {
            var trim = text.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace("\t", string.Empty);
            return Regex.Replace(trim, @"https:\/\/t.co.*$", string.Empty);
        }

        private async Task<bool> UpdateToken(bool force = false)
        {
            if (force || _lastRequest.AddMinutes(20) <= DateTimeOffset.Now)
            {
                _client.GuestToken = null;
                var token = await PollyHelper.WebFallbackAsync(() => _client.GetGuestTokenAsync());

                if (token != null)
                {
                    _client.GuestToken = token.Value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}