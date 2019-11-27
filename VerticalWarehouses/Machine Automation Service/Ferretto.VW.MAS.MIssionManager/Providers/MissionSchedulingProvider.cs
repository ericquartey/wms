using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionSchedulingProvider> logger;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            IEventAggregator eventAggregator,
            IMissionsDataProvider missionsDataProvider,
            IBaysDataProvider baysDataProvider,
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService, // this is here just for simulation purposes
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationssDataService, // this is here just for simulation purposes
            ILogger<MissionSchedulingProvider> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.missionOperationsDataService = missionOperationssDataService ?? throw new ArgumentNullException(nameof(missionOperationssDataService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        public IEnumerable<Mission> GetAllWmsMissions()
        {
            return this.missionsDataProvider.GetAllWmsMissions();
        }

        #endregion

        #region Methods

        public async Task QueueBayMissionAsync(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);

#if FALSE
            // Stefano, puoi aggiungere il tuo codice qui, per cominciare il flusso di gestione missioni.

            // ricorda di gestire il NotificationMessage "MissionOperationCompleted" che arriva dalla UI
            // e che al momento viene già inoltrato al WMS per segnalare la chiusura di una operazione

            throw new NotImplementedException();
#else
            await this.MOCK_ScheduleMissionsAsync(targetBayNumber);
#endif
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            throw new NotImplementedException();
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission()
        {
            throw new NotImplementedException();
        }

        private void NotifyAssignedMissionOperationChanged(
            BayNumber bayNumber,
            int? missionId,
            int? missionOperationId,
            int pendingMissionsCount)
        {
            var data = new AssignedMissionOperationChangedMessageData
            {
                BayNumber = bayNumber,
                MissionId = missionId,
                MissionOperationId = missionOperationId,
                PendingMissionsCount = pendingMissionsCount,
            };

            var notificationMessage = new NotificationMessage(
                data,
                $"Mission operation assigned to bay {bayNumber} has changed.",
                MessageActor.Any,
                MessageActor.MachineManager,
                MessageType.AssignedMissionOperationChanged,
                bayNumber);

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);
        }

        #endregion
    }
}
