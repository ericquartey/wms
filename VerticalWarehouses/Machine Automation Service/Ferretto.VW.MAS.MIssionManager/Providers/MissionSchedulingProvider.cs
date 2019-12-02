using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly CommandEvent commandEvent;

        private readonly ILogger<MissionSchedulingService> logger;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.commandEvent = eventAggregator.GetEvent<CommandEvent>();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            throw new NotImplementedException();
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
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

            this.logger.LogDebug($"New mission for loading unit {loadingUnitId} to bay {targetBayNumber}.");

            this.commandEvent.Publish(
                new CommandMessage(
                    data,
                    $"Wms mission requested; loadingUnitId {loadingUnitId}; Bay {targetBayNumber}",
                    MessageActor.MissionManager,
                    MessageActor.MissionManager,
                    MessageType.MoveLoadingUnit,
                    targetBayNumber));
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
