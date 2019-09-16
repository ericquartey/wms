using System.Collections.Generic;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    public interface ITorqueCurrentMeasurementsDataProvider
    {
        #region Methods

        TorqueCurrentMeasurementSession AddMeasurementSession(int? loadingUnitId, decimal? loadedNetWeight);

        TorqueCurrentSample AddSample(int sessionId, decimal value, System.DateTime timeStamp, System.DateTime requestTimeStamp);

        IEnumerable<TorqueCurrentMeasurementSession> GetAllMeasurementSessions();

        TorqueCurrentMeasurementSession GetMeasurementSession(int id);

        #endregion
    }
}
