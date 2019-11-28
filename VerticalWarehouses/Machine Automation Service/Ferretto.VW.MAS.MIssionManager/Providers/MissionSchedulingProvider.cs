using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionSchedulingProvider> logger;

        private readonly IMachineMissionsProvider machineMissionsProvider;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IMachineMissionsProvider missionsProvider,
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingProvider> logger)
        {
            this.machineMissionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            throw new NotImplementedException();
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            throw new NotImplementedException();
        }

        public Task QueueBayMissionAsync(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            var data = new MoveLoadingUnitMessageData(
                MissionType.WMS,
                LoadingUnitLocation.LoadingUnit,
                LoadingUnitLocation.NoLocation,
                null,
                null,
                loadingUnitId);

            data.WmsId = wmsMissionId;
            data.TargetBay = targetBayNumber;

            try
            {
                this.machineMissionsProvider.TryCreateMachineMission(FSMType.MoveLoadingUnit, data, targetBayNumber, out var missionId);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to start Move Loading Unit Wms mission: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
