using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services.Interfaces
{
    public interface IMachineModeService
    {
        #region Properties

        MachineMode MachineMode { get; }

        MachineModeChangedPubSubEvent MachineModeChangedEvent { get; }

        MachinePowerState MachinePower { get; }

        #endregion

        #region Methods

        Task PowerOffAsync();

        Task PowerOnAsync();

        #endregion
    }
}
