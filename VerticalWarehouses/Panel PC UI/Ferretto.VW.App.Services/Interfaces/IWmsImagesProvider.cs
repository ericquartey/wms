using System.IO;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IWmsImagesProvider
    {
        #region Methods

        Task<Stream> GetImageAsync(string imageKey);

        #endregion
    }
}
