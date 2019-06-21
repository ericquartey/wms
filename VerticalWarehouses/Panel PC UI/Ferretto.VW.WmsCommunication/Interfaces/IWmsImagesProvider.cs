using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.WmsCommunication.Interfaces
{
    public interface IWmsImagesProvider
    {
        #region Methods

        Task<FileResponse> GetImageAsync(string imageCode);

        #endregion
    }
}
