using System.Threading.Tasks;

namespace Ferretto.WMS.App.Core.Interfaces
{
    public interface IFileProvider
    {
        #region Methods

        Task<IImageFile> DownloadAsync(string key);

        Task<string> UploadAsync(string imagePath);

        #endregion
    }
}
