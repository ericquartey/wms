using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public class ServiceBase
    {
        #region Constructors

        protected ServiceBase()
        {
        }

        #endregion Constructors

        #region Properties

        public Func<Task<string>> RetrieveAuthorizationToken { get; set; }

        #endregion Properties

        #region Methods

        // Called by implementing swagger client classes
        protected async Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken cancellationToken)
        {
            var msg = new HttpRequestMessage();

            if (this.RetrieveAuthorizationToken != null)
            {
                var token = await this.RetrieveAuthorizationToken();
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return msg;
        }

        #endregion Methods
    }
}
