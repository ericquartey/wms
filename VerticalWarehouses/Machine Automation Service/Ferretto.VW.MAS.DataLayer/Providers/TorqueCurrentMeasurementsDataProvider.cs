using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public sealed class TorqueCurrentMeasurementsDataProvider : ITorqueCurrentMeasurementsDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public TorqueCurrentMeasurementsDataProvider(DataLayerContext dataContext)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public TorqueCurrentMeasurementSession AddMeasurementSession(int? loadingUnitId, double? loadedNetWeight)
        {
            if (loadedNetWeight.HasValue && !loadingUnitId.HasValue)
            {
                throw new InvalidOperationException(Resources.TorqueCurrent.ResourceManager.GetString("CannotSpecifyLoadedNet", CommonUtils.Culture.Actual));
            }

            var entry = this.dataContext.TorqueCurrentMeasurementSessions.Add(
                new TorqueCurrentMeasurementSession
                {
                    LoadedNetWeight = loadedNetWeight ?? 0,
                    LoadingUnitId = loadingUnitId,
                });

            this.dataContext.SaveChanges();

            return entry.Entity;
        }

        public TorqueCurrentSample AddSample(int sessionId, double value, DateTime timeStamp, DateTime requestTimeStamp)
        {
            var entry = this.dataContext.TorqueCurrentSamples.Add(
                new TorqueCurrentSample
                {
                    MeasurementSessionId = sessionId,
                    Value = value,
                    TimeStamp = timeStamp,
                    RequestTimeStamp = requestTimeStamp,
                });

            this.dataContext.SaveChanges();

            return entry.Entity;
        }

        public IEnumerable<TorqueCurrentMeasurementSession> GetAllMeasurementSessions()
        {
            return this.dataContext.TorqueCurrentMeasurementSessions.ToArray();
        }

        public TorqueCurrentMeasurementSession GetMeasurementSession(int id)
        {
            var session = this.dataContext.TorqueCurrentMeasurementSessions
                .SingleOrDefault(s => s.Id == id);
            if (session is null)
            {
                throw new EntityNotFoundException(id);
            }

            return session;
        }

        #endregion
    }
}
