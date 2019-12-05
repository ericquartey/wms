using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.FiniteStateMachines;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MockedMissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionSchedulingProvider> logger;

        private readonly IMachineMissionsProvider machineMissionsProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService;

        #endregion

        #region Constructors

        public MockedMissionSchedulingProvider(
            IEventAggregator eventAggregator,
            IMissionsDataProvider missionsDataProvider,
            IBaysDataProvider baysDataProvider,
            IMachineMissionsProvider missionsProvider,
            WMS.Data.WebAPI.Contracts.IMissionsDataService missionsDataService,
            WMS.Data.WebAPI.Contracts.IMissionOperationsDataService missionOperationssDataService,
            ILogger<MissionSchedulingProvider> logger)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.machineMissionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.missionsDataService = missionsDataService ?? throw new ArgumentNullException(nameof(missionsDataService));
            this.missionOperationsDataService = missionOperationssDataService ?? throw new ArgumentNullException(nameof(missionOperationssDataService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(eventAggregator));
        }

        #endregion

        #region Methods

        public IEnumerable<DataModels.Mission> GetAllWmsMissions()
        {
            return this.missionsDataProvider.GetAllWmsMissions();
        }

        public void GetPersistedMissions()
        {
            var missions = this.missionsDataProvider.GetAllExecutingMissions();
            foreach (var mission in missions)
            {
                this.machineMissionsProvider.AddMission(mission, mission.FsmId);
            }
        }

        public async Task MOCK_ScheduleMissionsAsync(BayNumber bayNumber)
        {
            // 1. prendi le missioni salvate in locale per la baia corrente
            var activeMissions = this.missionsDataProvider.GetAllActiveMissionsByBay(bayNumber);

            // 2. tra tutte, scegli quella con priorità più alta
            var nextMission = activeMissions
                .OrderBy(m => m.Priority)
                .ThenBy(m => m.CreationDate)
                .First();

            if (!nextMission.WmsId.HasValue)
            // non è una missione WMS
            {
                this.logger.LogInformation($"Bay {bayNumber}: completing non-wms mission.");
                await Task.Delay(5000);
                this.missionsDataProvider.SetStatus(nextMission.Id, MissionStatus.Completed);

                // richiama la procedura
                await this.MOCK_ScheduleMissionsAsync(bayNumber);

                return;
            }

            // 3. chiedo la missione al WMS per ottenere le sue operazioni
            var wmsMission = await this.missionsDataService.GetByIdAsync(nextMission.WmsId.Value);
            if (wmsMission.Operations.Any(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New)
                &&
                wmsMission.Operations.All(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Executing))
            // c'è almeno una nuova operazione e nessun'altra è in esecuzione
            {
                // NOTA: assumo che non ci siano operazioni in baia (perchè nessuna operazione è in esecuzione)

                // scelgo l'operazione con priorità più alta
                var nextOperation = wmsMission.Operations
                    .Where(o => o.Status == WMS.Data.WebAPI.Contracts.MissionOperationStatus.New)
                    .OrderBy(m => m.Priority)
                    .First();

                // (qui muovo il cassetto in baia, se non è ancora presente)
                this.logger.LogInformation($"Bay {bayNumber}: moving loading unit {wmsMission.LoadingUnitId} to bay.");
                await Task.Delay(5000);
                this.logger.LogInformation($"Bay {bayNumber}: loading unit {wmsMission.LoadingUnitId} is in bay!!!");

                // salvo in locale che la missione è assegnata alla baia
                this.baysDataProvider.AssignWmsMission(bayNumber, nextMission.Id, nextOperation.Id);

                // salvo in locale il fatto che è in esecuzione (se non lo era già)
                this.missionsDataProvider.SetStatus(nextMission.Id, MissionStatus.Executing);

                // informo il WMS che ho preso in carico l'esecuzione dell'operazione
                await this.missionOperationsDataService.ExecuteAsync(nextOperation.Id);

                this.logger.LogInformation($"Bay {bayNumber}: mission {nextMission.Id} is now in executing state (operation {nextOperation.Id}).");

                // notifico la UI che c'è una nuova operazione da fare
                this.NotifyAssignedMissionOperationChanged(bayNumber, nextMission.Id, nextOperation.Id, activeMissions.Count());
            }
            if (wmsMission.Operations.Any(o => o.Status != WMS.Data.WebAPI.Contracts.MissionOperationStatus.Executing))
            // c'è una operazione in esecuzione
            {
                this.logger.LogInformation($"Bay {bayNumber}: already executing an operation.");
            }
            else
            // non ci sono più operazioni nuove o in esecuzione, la missione può terminare
            {
                this.logger.LogInformation($"Bay {bayNumber}: mission {nextMission.Id} completed.");

                // (qui lo scheduler dovrebbe accodare la chiamata di rientro)

                // salvo in locale il fatto che è completata
                this.missionsDataProvider.SetStatus(nextMission.Id, MissionStatus.Completed);

                // salvo in locale che non c'è più una missione assegnata alla baia
                this.baysDataProvider.ClearMission(bayNumber);
            }

            await Task.Delay(10000);
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            throw new NotImplementedException();
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);

            this.MOCK_ScheduleMissionsAsync(targetBayNumber);
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission()
        {
            throw new NotImplementedException();
        }

        public async Task ScheduleMissionsAsync(BayNumber bayNumber, bool restore = false)
        {
            throw new NotSupportedException();
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
