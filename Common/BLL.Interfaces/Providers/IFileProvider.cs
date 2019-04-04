using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IFileProvider
    {
        #region Methods

        Task<IStreamFile> DownloadAsync(string key);

        Task<string> UploadAsync(string imagePath);

        #endregion
    }
}
