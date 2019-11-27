using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed partial class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Methods

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

        #endregion
    }
}
