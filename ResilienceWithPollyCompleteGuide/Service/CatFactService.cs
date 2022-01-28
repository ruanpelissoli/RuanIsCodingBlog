using Polly;

namespace ResilienceWithPollyCompleteGuide.Service
{
    public class CatFactService : ICatFactService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAsyncPolicy _retryAndFallbackPolicy;

        private const int MaxRetriesAttempts = 5;

        public CatFactService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;

            var retryPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: MaxRetriesAttempts,
                    sleepDurationProvider: times => TimeSpan.FromMilliseconds(times * 300),
                    onRetry: (ex, span, retryCount, ctx) =>
                    {
                        Console.WriteLine($"Retrying for the {retryCount} time");
                    });

            var fallbackPolicy = Policy.Handle<HttpRequestException>()
                .FallbackAsync(async cancelationToken =>
                {
                    Console.WriteLine("Policy fallback reached. Service is down for good");

                    await Task.Run(() => Console.WriteLine("Maybe send to a dead letter queue or logging"), cancelationToken);
                });

            _retryAndFallbackPolicy = Policy.WrapAsync(fallbackPolicy, retryPolicy);
        }

        public async Task<string> GetDailyFact()
        {
            var httpClient = _httpClientFactory.CreateClient("CatFacts");

            string dailyCatFactResponse = string.Empty;

            var random = new Random();

            await _retryAndFallbackPolicy.ExecuteAsync(async () =>
            {
                if (random.Next(1, 3) == 1)
                    throw new HttpRequestException();

                dailyCatFactResponse = await httpClient.GetStringAsync("fact");
            });

            return dailyCatFactResponse;
        }
    }
}
