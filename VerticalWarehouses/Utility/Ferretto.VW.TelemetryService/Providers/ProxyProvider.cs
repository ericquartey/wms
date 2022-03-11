using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

        public static string Decrypt(string cipherText, string salt)
        {
            byte[] IV = Convert.FromBase64String(cipherText.Substring(0, 20));
            cipherText = cipherText.Substring(20).Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            var plainText = string.Empty;
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(salt, IV);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    plainText = System.Text.Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return plainText;
        }

        public IProxy? Get()
        {
            lock (this.dataContext)
            {
                var proxy = this.dataContext.Proxys.SingleOrDefault();
                if (proxy != null && !string.IsNullOrEmpty(proxy.PasswordHash) && !string.IsNullOrEmpty(proxy.PasswordSalt))
                {
                    proxy.PasswordHash = Decrypt(proxy.PasswordHash, proxy.PasswordSalt);
                }
                return proxy;
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
