using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Events

        event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        Bay Bay { get; }

        BayNumber BayNumber { get; }

        IEnumerable<Cell> Cells { get; }

        bool HasBayExternal { get; }

        bool HasCarousel { get; }

        bool HasShutter { get; }

        bool IsHoming { get; }

        bool IsShutterThreeSensors { get; }

        IEnumerable<LoadingUnit> Loadunits { get; }

        MachineMode MachineMode { get; }

        MachinePowerState MachinePower { get; }

        MachineStatus MachineStatus { get; }
        bool BayFirstPositionIsUpper { get; }

        #endregion

        #region Methods

        void ClearNotifications();

        Task OnUpdateServiceAsync();

        void Start();

        Task StopMovingByAllAsync();

        #endregion
    }
}
