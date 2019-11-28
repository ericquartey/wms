using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MissionsDataProvider : IMissionsDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<DataLayerService> logger;

        #endregion

        #region Constructors

        public MissionsDataProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            ILogger<DataLayerService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        LoadingUnitId = loadingUnitId,
                        BayNumber = bayNumber
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation("Created internal MAS bay mission.");

                return entry.Entity;
            }
        }

        public Mission CreateBayMission(int loadingUnitId, BayNumber bayNumber, int wmsId, int wmsPriority)
        {
            lock (this.dataContext)
            {
                var entry = this.dataContext.Missions.Add(
                    new Mission
                    {
                        WmsId = wmsId,
                        Priority = wmsPriority,
                        LoadingUnitId = loadingUnitId,
                        BayNumber = bayNumber,
                        CreationDate = DateTime.Now,
                    });

                this.dataContext.SaveChanges();

                this.logger.LogInformation("Created MAS bay mission from WMS mission id={wmsId}");

                return entry.Entity;
            }
        }

        public IEnumerable<Mission> GetAllActiveMissionsByBay(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .AsNoTracking()
                    .Where(m => m.BayNumber == bayNumber)
                    .Where(m => m.Status != MissionStatus.Completed && m.Status != MissionStatus.Aborted)
                    .ToArray();
            }
        }

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Missions
                    .AsNoTracking()
                    .Where(m => m.WmsId != null)
                    .ToArray();
            }
        }

        public Mission SetStatus(int id, MissionStatus status)
        {
            lock (this.dataContext)
            {
                var mission = this.dataContext.Missions.SingleOrDefault(m => m.Id == id);

                mission.Status = status;

                this.dataContext.SaveChanges();

                return mission;
            }
        }

        #endregion
    }
}
