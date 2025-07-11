﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    public class ServiceBase
    {
        #region Constructors

        protected ServiceBase()
        {
        }

        #endregion

        #region Properties

        public Func<Task<string>> RetrieveAuthorizationToken { get; set; }

        #endregion

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

        #endregion
    }
}
