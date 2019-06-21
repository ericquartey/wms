using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Unity;

namespace Ferretto.VW.WmsCommunication
{
    public class WmsImagesProvider : IWmsImagesProvider
    {
        #region Fields

        private IUnityContainer container;

        private IImagesDataService imagesDataService;

        #endregion

        #region Constructors

        public WmsImagesProvider(IUnityContainer container, Uri wmsConnectionString)
        {
            this.container = container;
            this.imagesDataService = DataServiceFactory.GetService<IImagesDataService>(wmsConnectionString);
        }

        #endregion

        #region Methods

        public async Task<FileResponse> GetImageAsync(string imageCode)
        {
            var returnValue = await this.imagesDataService.DownloadAsync(imageCode);
            return returnValue;
        }

        #endregion
    }
}
