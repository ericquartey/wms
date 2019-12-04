using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Methods

        Task StopMovingByAllAsync();

        #endregion
    }
}
