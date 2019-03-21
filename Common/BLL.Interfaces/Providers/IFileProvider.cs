using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Microsoft.AspNetCore.Http;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IFileProvider
    {
        #region Methods

        Task<IImageFile> DownloadAsync(string key);

        Task<string> UploadAsync(IFormFile model);

        #endregion
    }
}
