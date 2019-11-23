using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IBayManager
    {
        #region Properties

        double ChainPosition { get; }

        MachineIdentity Identity { get; }

        #endregion

        #region Methods

        Task<Bay> GetBayAsync();

        Task InitializeAsync();

        #endregion
    }
}
