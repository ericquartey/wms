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

namespace Ferretto.VW.WmsCommunication
{
    public class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private const int MAX_CACHING_TIME_IN_MINUTES = 10;

        private const int MAX_STORED_IMAGES = 3;

        private readonly IUnityContainer container;

        private readonly IImagesDataService imagesDataService;

        private readonly Logger logger;

        private ObjectCache cache;

        private FileStream copyStream;

        private int counter;

        private Image returnImage;

        #endregion

        #region Constructors

        public WmsImagesProvider(IUnityContainer container, Uri wmsConnectionString)
        {
            this.container = container;
            this.imagesDataService = DataServiceFactory.GetService<IImagesDataService>(wmsConnectionString);
            this.logger = LogManager.GetCurrentClassLogger();
            this.LoadStoredImages();
            this.logger.Log(LogLevel.Debug, "WmsImagesProvider ctor ended");
            this.cache = MemoryCache.Default;
        }

        #endregion

        #region Properties

        public Dictionary<string, string> StoredImages { get; set; }

        #endregion

        #region Methods

        public async Task<Stream> GetImageAsync(string imageCode)
        {
            this.copyStream?.Dispose();
            var cachedObject = this.cache[imageCode] as Stream;
            if (cachedObject == null)
            {
                using (var response = await this.imagesDataService.DownloadAsync(imageCode))
                {
                    this.copyStream = new FileStream(string.Concat("./Images/", imageCode), FileMode.Create);
                    await response.Stream.CopyToAsync(this.copyStream);
                    var policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(MAX_CACHING_TIME_IN_MINUTES);
                    cachedObject = this.copyStream;
                    this.cache.Set(imageCode, cachedObject, policy);
                }
            }
            return cachedObject;
        }

        //public async Task<Image> GetImageAsync(string imageCode)
        //{
        //    this.returnImage?.Dispose();
        //    this.returnImage = null;
        //    var path = string.Empty;
        //    try
        //    {
        //        if (this.StoredImages.ContainsKey(imageCode))
        //        {
        //            this.StoredImages.TryGetValue(imageCode, out path);
        //        }
        //        else
        //        {
        //            path = await this.DownloadImageAsync(imageCode);
        //        }

        //        this.returnImage = (Image)Image.FromFile(path).Clone();
        //        this.logger.Log(LogLevel.Debug, $"GetImageAsync ended {this.counter}");
        //    }
        //    catch (Exception ex)
        //    {
        //        this.logger.Error(ex, $"Get Image Async error catched {this.counter}");
        //    }
        //    this.counter++;
        //    return this.returnImage;
        //}

        private void AddImageLogRecord(string imageCode)
        {
            try
            {
                var s = new string[] { string.Concat(DateTime.Now.ToString(), "-", imageCode) };
                File.AppendAllLines("./Images/imagelog.txt", s);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Ass Image Log Record error catched {this.counter}");
            }
            this.logger.Log(LogLevel.Debug, $"AddImageLogRecord ended {this.counter}");
        }

        private bool AreTooManyImagesStored()
        {
            var files = Directory.GetFiles("./Images/", "*.jpg");
            this.logger.Log(LogLevel.Debug, $"AreTooManyImagesStored ctor ended {this.counter}");
            return files.Length >= MAX_STORED_IMAGES;
        }

        private Task DeleteOlderImage()
        {
            try
            {
                var lines = File.ReadAllLines("./Images/imagelog.txt");
                var dates = new Dictionary<DateTime, string>();
                for (var i = 0; i < lines.Length; i++)
                {
                    var s = lines[i].Split('-');
                    DateTime.TryParse(s[0], out var result);
                    dates.Add(result, s[1]);
                }
                var itemToRemove = dates.OrderBy(x => x.Key).First();
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage GC calls start {this.counter}");
                this.returnImage?.Dispose();
                this.returnImage = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage GC calls ended {this.counter}");
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage delete image start {this.counter}");
                if (File.Exists(string.Concat("./Images/", itemToRemove.Value)))
                {
                    this.logger.Log(LogLevel.Debug, $"DeleteOlderImage file EXISTS {this.counter}");
                    File.Delete(string.Concat("./Images/", itemToRemove.Value));
                }
                else
                {
                    this.logger.Log(LogLevel.Debug, $"DeleteOlderImage file NOT EXISTS {this.counter}");
                }
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage delete image ended {this.counter}");
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage delete log start {this.counter}");
                File.Delete("./Images/imagelog.txt");
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage delete log ended {this.counter}");
                this.StoredImages.Remove(itemToRemove.Value);
                var newLines = new List<string>();
                for (var i = 1; i < dates.Count; i++)
                {
                    var s = string.Concat(dates.ElementAt(i).Key, "-", dates.ElementAt(i).Value);
                    newLines.Add(s);
                }
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage write log start {this.counter}");
                File.WriteAllLines("./Images/imagelog.txt", newLines);
                this.logger.Log(LogLevel.Debug, $"DeleteOlderImage write log ended {this.counter}");
            }
            catch (IOException ex)
            {
                this.logger.Error(ex, $"Delete Older Image IOException catched {this.counter}");
            }
            catch (NotSupportedException suppex)
            {
                this.logger.Error(suppex, $"Delete Older Image NotSupportedException catched {this.counter}");
            }
            catch (UnauthorizedAccessException accex)
            {
                this.logger.Error(accex, $"Delete Older Image UnauthorizedAccessException catched {this.counter}");
            }
            catch (Exception genex)
            {
                this.logger.Error(genex, $"Delete Older Image Exception catched {this.counter}");
            }
            this.logger.Log(LogLevel.Debug, $"DeleteOlderImage ended {this.counter}");
            return Task.CompletedTask;
        }

        private async Task<string> DownloadAndRegisterImageAsync(string imageCode)
        {
            try
            {
                var path = string.Concat("./Images/", imageCode);
                using (var response = await this.imagesDataService.DownloadAsync(imageCode))
                {
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        response.Stream.CopyTo(fileStream);
                        this.AddImageLogRecord(imageCode);
                        this.StoredImages.Add(imageCode, path);
                        this.logger.Log(LogLevel.Debug, $"DownloadAndRegisterImageAsync ended {this.counter}");
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Download And Register Image error catched {this.counter}");
                return null;
            }
        }

        private async Task<string> DownloadImageAsync(string imageCode)
        {
            var returnValue = "";
            if (this.AreTooManyImagesStored())
            {
                await this.DeleteOlderImage();
                returnValue = await this.DownloadAndRegisterImageAsync(imageCode);
            }
            else
            {
                returnValue = await this.DownloadAndRegisterImageAsync(imageCode);
            }
            this.logger.Log(LogLevel.Debug, $"DownloadImageAsync ended {this.counter}");
            return returnValue;
        }

        private void LoadStoredImages()
        {
            this.StoredImages = new Dictionary<string, string>();
            var files = Directory.GetFiles("./Images/", "*.jpg");
            for (var i = 0; i < files.Length; i++)
            {
                this.StoredImages.Add(files[i].Remove(0, 9), files[i]);
            }
            this.logger.Log(LogLevel.Debug, $"LoadStoredImages ended {this.counter}");
        }

        #endregion
    }
}
