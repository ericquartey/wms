using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Proxy = Ferretto.ServiceDesk.Telemetry.Proxy;

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

        public static string Decrypt(string cipherText, string salt)
        {
            var iv = new byte[16];
            Buffer.BlockCopy(Convert.FromBase64String(cipherText), 0, iv, 0, iv.Length);
            using (var aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                using (var decryptor = aesAlg.CreateDecryptor(Convert.FromBase64String(salt), iv))
                {
                    byte[] encrypted = Convert.FromBase64String(cipherText);
                    using (var msDecrypt = new MemoryStream(encrypted, iv.Length, encrypted.Length - iv.Length))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var resultStream = new MemoryStream())
                            {
                                csDecrypt.CopyTo(resultStream);
                                return System.Text.Encoding.UTF8.GetString(resultStream.ToArray());
                            }
                        }
                    }
                }
            }
        }

        public Proxy Get()
        {
            lock (this.dataContext)
            {
                var proxy = this.dataContext.Proxys.SingleOrDefault();

                if (proxy != null && !string.IsNullOrEmpty(proxy.Url))
                {
                    var webProxy = new Proxy() { User = proxy.User, Url = proxy.Url, PasswordHash = proxy.PasswordHash, PasswordSalt = proxy.PasswordSalt };

                    return webProxy;
                }

                return null;
            }
        }

        public WebProxy GetWebProxy()
        {
            lock (this.dataContext)
            {
                var proxy = this.dataContext.Proxys.SingleOrDefault();

                if (proxy != null && !string.IsNullOrEmpty(proxy.Url))
                {
                    var webProxy = new WebProxy(proxy.Url) { Credentials = new NetworkCredential(proxy.User, Decrypt(proxy.PasswordHash, proxy.PasswordSalt)) };

                    return webProxy;
                }

                return null;
            }
        }

        public void SaveAsync(Proxy proxy)
        {
            if (proxy is null)
            {
                proxy = new Proxy() { PasswordHash = null, PasswordSalt = null, Url = null, User = null };
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
