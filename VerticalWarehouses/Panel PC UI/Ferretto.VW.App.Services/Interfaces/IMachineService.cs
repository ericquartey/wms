using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Properties

        bool IsHoming { get; }

        #endregion

        #region Methods

        Task StopMovingByAllAsync();

        #endregion
    }
}
