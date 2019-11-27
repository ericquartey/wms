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

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionSchedulingProvider> logger;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IEventAggregator eventAggregator,
            ILogger<MissionSchedulingProvider> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int? wmsMissionId)
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

            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
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
