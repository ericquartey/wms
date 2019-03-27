using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IFileProvider
    {
        #region Methods

        Task<IImageFile> DownloadAsync(string key);

        Task<string> UploadAsync(string imagePath, IFormFile model);

        #endregion
    }
}
