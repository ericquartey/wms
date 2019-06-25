using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.WmsCommunication.Interfaces
{
    public interface IWmsImagesProvider
    {
        #region Properties

        Dictionary<string, string> StoredImages { get; set; }

        #endregion

        #region Methods

        Task<Stream> GetImageAsync(string imageCode);

        #endregion
    }
}
