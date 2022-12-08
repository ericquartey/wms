using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IStatisticsDataProvider
    {
        #region Methods

        bool AddInverterStatistics(double workingHours, double operationHours, double peakHeatSinkTemperature, double peakInsideTemperature, double averageRMSCurrent, double averageActivePower);

        int ConfirmAndCreateNew();

        MachineStatistics GetActual();

        IEnumerable<MachineStatistics> GetAll();

        MachineStatistics GetById(int id);

        MachineStatistics GetLastConfirmed();

        int MissionTotalNumber();

        int PurgeInverterStatistics();

        double TotalDistance();

        #endregion
    }
}
