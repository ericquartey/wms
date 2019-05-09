using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BLL.Interfaces.Providers
{
    public interface IFileProvider
    {
        #region Methods

        Task<IOperationResult<IStreamFile>> DownloadAsync(string key);

        Task<IOperationResult<string>> UploadAsync(string imagePath);

        #endregion
    }
}
