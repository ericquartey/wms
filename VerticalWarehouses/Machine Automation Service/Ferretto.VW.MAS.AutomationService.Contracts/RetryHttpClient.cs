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

        private static readonly Random Random = new Random();

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
            var jitterSeconds = Random.NextDouble();

            Func<int, TimeSpan> sleepDurationProvider = count =>
            {
                var timeout = TimeSpan.FromSeconds(jitterSeconds + Math.Pow(count / 3.0, 2));

                System.Diagnostics.Debug.WriteLine($"Request: {request.Method} {request.RequestUri} (retry #{count}). Retrying in {timeout.TotalSeconds} seconds ...");

                return timeout;
            };

            return await Policy
              .Handle<HttpRequestException>()
              .OrResult<HttpResponseMessage>(response => IsTooManyRequests(response) || IsServerError(response))
               .WaitAndRetryAsync(0, sleepDurationProvider)
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
            newRequest.Content = request.Content;
            return newRequest;
        }

        private static bool IsServerError(HttpResponseMessage response)
        {
            var isServerError = (int)response.StatusCode / 100 == 5;

            if (isServerError)
            {
                System.Diagnostics.Debug.WriteLine($"Response: {(int)response.StatusCode} {response.ReasonPhrase} ({response.RequestMessage.RequestUri})");
            }

            return isServerError;
        }

        private static bool IsTooManyRequests(HttpResponseMessage response)
        {
            var isTooManyRequests = (int)response.StatusCode == 429;

            if (isTooManyRequests)
            {
                System.Diagnostics.Debug.WriteLine($"Response: {(int)response.StatusCode} {response.ReasonPhrase} ({response.RequestMessage.RequestUri})");
            }

            return isTooManyRequests;
        }

        #endregion
    }
}
