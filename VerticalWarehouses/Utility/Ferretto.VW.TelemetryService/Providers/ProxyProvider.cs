using System;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ProxyProvider : IProxyProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        #endregion

        #region Constructors

        public ProxyProvider(IDataContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public IProxy? Get()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Proxys.SingleOrDefault();
            }
        }

        public void SaveAsync(IProxy proxy)
        {
            if (proxy is null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            lock (this.dataContext)
            {
                var proxyDb = this.dataContext.Proxys.SingleOrDefault();
                if (proxyDb is null)
                {
                    proxyDb = new Data.Proxy()
                    {
                        PasswordHash = proxy.PasswordHash,
                        PasswordSalt = proxy.PasswordSalt,
                        Url = proxy.Url,
                        User = proxy.User,
                    };
                    this.dataContext.Proxys.Add(proxyDb);
                }
                else
                {
                    proxyDb.PasswordHash = proxy.PasswordHash;
                    proxyDb.PasswordSalt = proxy.PasswordSalt;
                    proxyDb.Url = proxy.Url;
                    proxyDb.User = proxy.User;

                    this.dataContext.Proxys.Update(proxyDb);
                }
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
