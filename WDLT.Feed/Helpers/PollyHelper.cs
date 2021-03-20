using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.Timeout;

namespace WDLT.Feed.Helpers
{
    public static class PollyHelper
    {
        public static Task<T> WebFallbackAsync<T>(Func<Task<T>> func)
        {
            var fallbackPolicy = Policy<T>
                .Handle<HttpRequestException>()
                .Or<TimeoutRejectedException>()
                .Or<AggregateException>()
                .FallbackAsync(default(T));

            return fallbackPolicy.ExecuteAsync(func);
        }
    }
}