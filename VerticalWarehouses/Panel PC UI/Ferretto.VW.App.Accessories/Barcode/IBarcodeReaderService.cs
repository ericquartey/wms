using System.Threading.Tasks;

namespace Ferretto.VW.App.Accessories
{
    public interface IBarcodeReaderService
    {
        #region Methods

        void Disable();

        Task StartAsync();

        #endregion
    }
}
