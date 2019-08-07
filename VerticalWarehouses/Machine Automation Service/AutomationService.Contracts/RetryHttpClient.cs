using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class RetryHttpClient : HttpClient
    {
        #region Fields

        private const int DefaultMaximumRetries = 3;

        private static readonly Random random = new Random();

        private int maximumRetries = DefaultMaximumRetries;

        #endregion

        #region Properties

        public int MaximumRetries
        {
            get => this.maximumRetries;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "The value cannot be negative.");
                }

                this.maximumRetries = value;
            }
        }

        #endregion

        #region Methods

        public new async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            var jitterSeconds = random.NextDouble();

            System.Diagnostics.Debug.WriteLine($"Request: {request.Method} {request.RequestUri}");
            Func<int, TimeSpan> sleepDurationProvider = count =>
            {
                System.Diagnostics.Debug.WriteLine($"Request: {request.Method} {request.RequestUri} (retry #{count})");
                return TimeSpan.FromSeconds(jitterSeconds + Math.Pow(count, 2));
            };
            ;

            return await Policy
              .Handle<HttpRequestException>()
              .OrResult<HttpResponseMessage>(response => IsTooManyRequests(response) || IsServerError(response))
              .WaitAndRetryAsync(this.MaximumRetries, sleepDurationProvider)
              .ExecuteAsync(async () => await base.SendAsync(CopyRequest(request), completionOption, cancellationToken));
        }

        private static HttpRequestMessage CopyRequest(HttpRequestMessage request)
        {
            var newRequest = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                if (newRequest.Headers.Contains(header.Key))
                {
                    newRequest.Headers.Remove(header.Key);
                }

                newRequest.Headers.Add(header.Key, header.Value);
            }

            foreach (var property in request.Properties)
            {
                if (newRequest.Properties.ContainsKey(property.Key))
                {
                    newRequest.Properties.Remove(property.Key);
                }
                newRequest.Properties.Add(property.Key, property.Value);
            }

            newRequest.Version = request.Version;

            return newRequest;
        }

        private static bool IsServerError(HttpResponseMessage response)
        {
            System.Diagnostics.Debug.WriteLine($"Response is {response.StatusCode}: {response.RequestMessage.Method} {response.RequestMessage.RequestUri}");

            return (int)response.StatusCode / 100 == 5;
        }

        private static bool IsTooManyRequests(HttpResponseMessage response)
        {
            return (int)response.StatusCode == 429;
        }

        #endregion
    }
}
