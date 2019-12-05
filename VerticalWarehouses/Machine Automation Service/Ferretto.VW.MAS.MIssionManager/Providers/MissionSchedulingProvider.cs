using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly ILogger<MissionSchedulingService> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IEventAggregator eventAggregator,
            IMissionsDataProvider missionsDataProvider,
            ILogger<MissionSchedulingService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            this.logger.LogDebug($"Queuing local mission for loading unit {loadingUnitId} to bay {targetBayNumber}.");

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber);

            this.NotifyNewMachineMissionAvailable(mission);
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.logger.LogDebug($"Queuing WMS mission for loading unit {loadingUnitId} to bay {targetBayNumber}.");

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);

            this.NotifyNewMachineMissionAvailable(mission);
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission()
        {
            throw new NotImplementedException();
        }

        private void NotifyNewMachineMissionAvailable(DataModels.Mission mission)
        {
            var notificationMessage = new NotificationMessage(
                null,
                $"New machine mission available for bay {mission.TargetBay}.",
                MessageActor.MachineManager,
                MessageActor.MachineManager,
                MessageType.NewMachineMissionAvailable,
                mission.TargetBay);

            this.notificationEvent.Publish(notificationMessage);
        }

        #endregion
    }
}
