using System.Collections.Generic;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.InverterDriver.Interface.Services
{
    public interface IInverterService
    {
        #region Properties

        IEnumerable<IInverterStatusBase> GetStatuses { get; }

        int StatusesCount { get; }

        #endregion

        #region Methods

        bool AddStatus(InverterIndex inverterIndex, IInverterStatusBase inverterStatus);

        bool TryGetValue(InverterIndex inverterIndex, out IInverterStatusBase inverterStatus);

        #endregion
    }
}
