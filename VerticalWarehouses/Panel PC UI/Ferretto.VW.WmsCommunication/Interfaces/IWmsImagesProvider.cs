using System.IO;
using System.Threading.Tasks;

namespace Ferretto.VW.WmsCommunication.Interfaces
{
    public interface IWmsImagesProvider
    {
        #region Methods

        Task<Stream> GetImageAsync(string imageCode);

        #endregion
    }
}
