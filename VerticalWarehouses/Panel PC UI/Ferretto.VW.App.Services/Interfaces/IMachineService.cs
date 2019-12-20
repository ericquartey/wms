using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Properties

        Bay Bay { get; }

        MAS.AutomationService.Contracts.BayNumber BayNumber { get; }

        IEnumerable<Cell> Cells { get; }

        bool HasShutter { get; }

        bool IsHoming { get; }

        bool IsShutterThreeSensors { get; }
        
        IEnumerable<LoadingUnit> LoadUnits { get; }

        MachineStatus MachineStatus { get; }
        bool HasCarousel { get;  }
        bool HasBayExternal { get; }

        #endregion

        #region Methods

        void ServiceStart();

        Task StopMovingByAllAsync();

        #endregion
    }
}
