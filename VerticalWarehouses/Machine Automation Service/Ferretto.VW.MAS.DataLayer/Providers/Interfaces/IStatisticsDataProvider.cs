using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IStatisticsDataProvider
    {
        #region Methods

        int ConfirmAndCreateNew();

        MachineStatistics GetActual();

        IEnumerable<MachineStatistics> GetAll();

        MachineStatistics GetById(int id);

        MachineStatistics GetLastConfirmed();

        int MissionTotalNumber();

        double TotalDistance();

        #endregion
    }
}
