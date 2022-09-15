using System;
using System.Linq;
using System.Net;
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

        public ServiceDesk.Telemetry.Proxy Get()
        {
            lock (this.dataContext)
            {
                var proxy = this.dataContext.Proxys.SingleOrDefault();

                if (proxy is null)
                {
                    return null;
                }

                var telemetryProxy = new ServiceDesk.Telemetry.Proxy()
                {
                    Url = proxy.Url,
                    User = proxy.User,
                    PasswordHash = proxy.PasswordHash,
                    PasswordSalt = proxy.PasswordSalt,
                };
                return telemetryProxy;
            }
        }

        public WebProxy GetWebProxy()
        {
            lock (this.dataContext)
            {
                var proxy = this.dataContext.Proxys.SingleOrDefault();

                if (proxy != null && !string.IsNullOrEmpty(proxy.Url) && !string.IsNullOrEmpty(proxy.PasswordHash))
                {
                    var psw = DecryptEncrypt.Decrypt(proxy.PasswordHash, proxy.PasswordSalt);
                    var webProxy = new WebProxy(proxy.Url) { Credentials = new NetworkCredential(proxy.User, psw) };

                    return webProxy;
                }

                return null;
            }
        }

        public void SaveAsync(IProxy? proxy)
        {
            if (proxy is null)
            {
                proxy = new Data.Proxy() { PasswordHash = null, PasswordSalt = null, Url = null, User = null };
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
