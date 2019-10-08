using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface ITorqueCurrentMeasurementsDataProvider
    {
        #region Methods

        TorqueCurrentMeasurementSession AddMeasurementSession(int? loadingUnitId, double? loadedNetWeight);

        TorqueCurrentSample AddSample(int sessionId, double value, System.DateTime timeStamp, System.DateTime requestTimeStamp);

        IEnumerable<TorqueCurrentMeasurementSession> GetAllMeasurementSessions();

        TorqueCurrentMeasurementSession GetMeasurementSession(int id);

        #endregion
    }
}
