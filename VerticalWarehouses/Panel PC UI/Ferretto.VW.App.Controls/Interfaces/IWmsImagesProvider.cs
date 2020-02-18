using System.Threading.Tasks;

namespace Ferretto.VW.App.Controls
{
    public interface IWmsImagesProvider
    {
        #region Methods

        Task<System.IO.Stream> GetImageAsync(string imageKey);

        #endregion
    }
}
