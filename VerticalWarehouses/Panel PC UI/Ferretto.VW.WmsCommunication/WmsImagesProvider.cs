using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Unity;
using NLog;
using System.Runtime.Caching;
using System.Collections.Specialized;

namespace Ferretto.VW.WmsCommunication
{
    public class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private const int MAX_CACHE_CAPACITY_MB = 200;

        private const int MAX_CACHING_TIME_IN_MINUTES = 720; // INFO 60 minutes * 12 hours (avarage machine's daily work load)

        private readonly IUnityContainer container;

        private readonly IImagesDataService imagesDataService;

        private readonly Logger logger;

        private MemoryCache cache;

        #endregion

        #region Constructors

        public WmsImagesProvider(IUnityContainer container, Uri wmsConnectionString)
        {
            this.container = container;
            this.imagesDataService = DataServiceFactory.GetService<IImagesDataService>(wmsConnectionString);
            this.logger = LogManager.GetCurrentClassLogger();

            var config = new NameValueCollection() { };
            config.Add("pollingInterval", "00:00:10");
            config.Add("physicalMemoryLimitPercentage", "0");
            config.Add("cacheMemoryLimitMegabytes", MAX_CACHE_CAPACITY_MB.ToString());
            this.cache = new MemoryCache("ImageCache", config);
        }

        #endregion

        #region Methods

        public async Task<Stream> GetImageAsync(string imageCode)
        {
            Stream cachedObject;
            if (imageCode != null)
            {
                cachedObject = this.cache[imageCode] as Stream;
                if (cachedObject == null)
                {
                    try
                    {
                        using (var response = await this.imagesDataService.DownloadAsync(imageCode))
                        {
                            var policy = new CacheItemPolicy();
                            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(MAX_CACHING_TIME_IN_MINUTES);
                            cachedObject = new MemoryStream(this.ReadFully(response.Stream));
                            this.cache.Set(imageCode, cachedObject, policy);
                        }
                    }
                    catch (Exception ex)
                    {
                        cachedObject = this.cache["rollback"] as Stream;
                        if (cachedObject == null)
                        {
                            var policy = new CacheItemPolicy();
                            policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(MAX_CACHING_TIME_IN_MINUTES);
                            var stream = new FileStream("./Images/Ingranaggio_quadrato.png", FileMode.Open);
                            cachedObject = new MemoryStream(this.ReadFully(stream));
                            this.cache.Set("rollback", cachedObject, policy);
                        }
                    }
                }
            }
            else
            {
                cachedObject = this.cache["rollback"] as Stream;
                if (cachedObject == null)
                {
                    var policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(MAX_CACHING_TIME_IN_MINUTES);
                    var stream = new FileStream("./Images/Divieto_Dark.png", FileMode.Open);
                    cachedObject = new MemoryStream(this.ReadFully(stream));
                    this.cache.Set("rollback", cachedObject, policy);
                }
            }
            return cachedObject;
        }

        public byte[] ReadFully(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        #endregion
    }
}
