using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Migrations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionOperationsProvider : IMissionOperationsProvider
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<MissionOperationsProvider> logger;

        private readonly IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        private readonly IMachinesWmsWebService machinesWmsWebService;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        #endregion

        #region Constructors

        public MissionOperationsProvider(
            IEventAggregator eventAggregator,
            IMissionSchedulingProvider missionSchedulingProvider,
            IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IMissionsDataProvider missionsDataProvider,
            IWmsSettingsProvider wmsSettingsProvider,
            IErrorsProvider errorsProvider,
            ILogger<MissionOperationsProvider> logger,
            IMachinesWmsWebService machinesWmsWebService,
            IMachineVolatileDataProvider machineVolatileDataProvider)
        {
            this.eventAggregator = eventAggregator;
            this.missionSchedulingProvider = missionSchedulingProvider;
            this.missionOperationsWmsWebService = missionOperationsWmsWebService;
            this.missionsDataProvider = missionsDataProvider;
            this.wmsSettingsProvider = wmsSettingsProvider;
            this.errorsProvider = errorsProvider;
            this.logger = logger;
            this.machinesWmsWebService = machinesWmsWebService;
            this.machineVolatileDataProvider = machineVolatileDataProvider;
        }

        #endregion

        #region Methods

        public async Task PostStates(int id)
        {
            if (id > 0 && this.wmsSettingsProvider.AlarmsToWmsOn) { 
                string myMachineModes = string.Empty;
                string myMachinePower = string.Empty;

                switch (this.machineVolatileDataProvider.Mode)
                {
                    case MachineMode.Automatic:
                        myMachineModes = "Automatico";
                        break;
                    case MachineMode.Manual:
                    case MachineMode.Manual2:
                    case MachineMode.Manual3:
                    case MachineMode.LoadUnitOperations:
                    case MachineMode.LoadUnitOperations2:
                    case MachineMode.LoadUnitOperations3:
                    case MachineMode.Test:
                    case MachineMode.Test2:
                    case MachineMode.Test3:
                    case MachineMode.Compact:
                    case MachineMode.Compact2:
                    case MachineMode.Compact3:
                    case MachineMode.Shutdown:
                    case MachineMode.FullTest:
                    case MachineMode.FirstTest:
                    case MachineMode.FullTest2:
                    case MachineMode.FirstTest2:
                    case MachineMode.FullTest3:
                    case MachineMode.FirstTest3:
                    case MachineMode.SwitchingToAutomatic:
                    case MachineMode.SwitchingToManual:
                    case MachineMode.SwitchingToManual2:
                    case MachineMode.SwitchingToManual3:
                    case MachineMode.SwitchingToShutdown:
                    case MachineMode.SwitchingToLoadUnitOperations:
                    case MachineMode.SwitchingToCompact:
                    case MachineMode.SwitchingToFullTest:
                    case MachineMode.SwitchingToFirstTest:
                    case MachineMode.SwitchingToLoadUnitOperations2:
                    case MachineMode.SwitchingToCompact2:
                    case MachineMode.SwitchingToFullTest2:
                    case MachineMode.SwitchingToFirstTest2:
                    case MachineMode.SwitchingToLoadUnitOperations3:
                    case MachineMode.SwitchingToCompact3:
                    case MachineMode.SwitchingToFullTest3:
                    case MachineMode.SwitchingToFirstTest3:
                        myMachineModes = "Manuale";
                        break;
                    default:
                        myMachineModes = string.Empty;
                        break;
                }

                switch (this.machineVolatileDataProvider.MachinePowerState)
                {
                    case MachinePowerState.Powered:
                        myMachinePower = "Accesa";
                        break;
                    default:
                        myMachinePower = "Spenta";
                        break;
                }

                await this.machinesWmsWebService.PostStatesAsync(id, new string[] { myMachinePower, myMachineModes });
            }
        }

        public async Task PostAlarms(int id) {
            if (id > 0 && this.wmsSettingsProvider.AlarmsToWmsOn) {
            
                List<string> errorsDescriptions = new List<string>();

                var error = this.errorsProvider
                    .GetErrors().Where(e => !e.ResolutionDate.HasValue);

                if (null != error && error.Any())
                {
                    //errorsDescriptions.Clear();
                    foreach (var item in error)
                    {
                        var description = "";

                        if (item.Description != null)
                        {
                            description = $"{item?.Description} ";
                        }
                        if (item?.AdditionalText != null)
                        {
                            description += $"{item?.AdditionalText}";
                        }

                        errorsDescriptions.Add(description);
                    }
                }
                else
                {
                    errorsDescriptions.Add(string.Empty);
                }

                await this.machinesWmsWebService.PostAlarmsAsync(id, errorsDescriptions);
            }
        }

        public async Task AbortAsync(int wmsId, string userName = null)
        {
            if (!this.wmsSettingsProvider.IsEnabled)
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsWmsWebService.AbortAsync(wmsId, userName);
            }
            catch (WmsWebApiException ex)
            {
                this.NegativeResult(ex);
            }
        }

        /// <summary>
        /// Marks the specified WMS mission as complete.
        /// </summary>
        /// <param name="wmsId">
        /// The identifier of the WMS mission to complete.
        /// </param>
        /// <param name="quantity">
        /// The product quantity that was involved.
        /// </param>
        /// <returns>
        /// Returns HTTP 200 if the completion request was successfully processed.
        /// </returns>
        public async Task CompleteAsync(int wmsId, double quantity, string printerName, string barcode = null, double wastedQuantity = 0, string toteBarcode = null, string userName = null, int? nrLabels = null)
        {
            try
            {
                if (this.wmsSettingsProvider.IsEnabled)
                {
                    //x double wastedQuantity = 0;
                    await this.missionOperationsWmsWebService.CompleteItemAsync(wmsId, quantity, wastedQuantity, printerName, null, barcode, toteBarcode, userName, nrLabels);
                }
                else
                {
                    var mission = this.missionsDataProvider.GetByWmsId(wmsId);
                    this.logger.LogWarning($"WMS is not enabled. Forcing recall of load unit {mission.LoadUnitId} from bay {mission.TargetBay}.");

                    this.missionSchedulingProvider.QueueRecallMission(mission.LoadUnitId, mission.TargetBay, MissionType.IN);
                }

                var messageData = new MissionOperationCompletedMessageData
                {
                    MissionOperationId = wmsId,
                };

                var notificationMessage = new NotificationMessage(
                    messageData,
                    "Mission Operation Completed",
                    MessageActor.MissionManager,
                    MessageActor.WebApi,
                    MessageType.MissionOperationCompleted,
                    BayNumber.None);

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }
            catch (WmsWebApiException ex)
            {
                this.NegativeResult(ex);
            }
        }

        public async Task<MissionOperation> GetByIdAsync(int wmsId)
        {
            return await this.missionOperationsWmsWebService.GetByIdAsync(wmsId);
        }

        //public async Task<MissionOperation> GetByAggregateAsync(int wmsId)
        //{
        //    return await this.missionOperationsWmsWebService.AggregateAsync(wmsId);
        //}

        public int GetCountByBay(BayNumber bayNumber)
        {
            return this.missionsDataProvider
                .GetAllActiveMissionsByBay(bayNumber)
                .Where(m => m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Completed
                    && m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Aborted
                    )
                .Count();
        }

        public async Task<IEnumerable<OperationReason>> GetOrdersAsync()
        {
            return await this.missionOperationsWmsWebService.GetAllOrdersAsync();
        }

        public async Task<IEnumerable<MissionOperation>> GetPutListsAsync(int machineId)
        {
            return await this.missionOperationsWmsWebService.GetPutListsAsync(machineId);
        }

        public async Task<IEnumerable<OperationReason>> GetReasonsAsync(MissionOperationType type)
        {
            return await this.missionOperationsWmsWebService.GetAllReasonsAsync(type);
        }

        public int GetUnitId(int missionId, BayNumber bayNumber)
        {
            try
            {
                return this.missionsDataProvider
                .GetAllActiveMissions()
                .Where(m => m.Id == missionId).LastOrDefault().LoadUnitId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// the UI informs mission manager that the operation is completed
        /// </summary>
        /// <param name="id">
        /// operation id
        /// </param>
        /// <param name="quantity">
        /// </param>
        /// <returns>
        /// </returns>
        public async Task PartiallyCompleteAsync(int wmsId, double quantity, double wastedQuantity, string printerName, bool emptyCompartment = false, bool fullCompartment = false, string userName = null, int? nrLabels = null)
        {
            if (!this.wmsSettingsProvider.IsEnabled)
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsWmsWebService.PartiallyCompleteAndRescheduleItemAsync(wmsId, quantity, wastedQuantity, printerName, emptyCompartment, fullCompartment, userName, nrLabels);

                var messageData = new MissionOperationCompletedMessageData
                {
                    MissionOperationId = wmsId,
                };

                var notificationMessage = new NotificationMessage(
                    messageData,
                    "Mission Operation Partially Completed",
                    MessageActor.MissionManager,
                    MessageActor.WebApi,
                    MessageType.MissionOperationCompleted,
                    BayNumber.None);

                this.eventAggregator
                    .GetEvent<NotificationEvent>()
                    .Publish(notificationMessage);
            }
            catch (WmsWebApiException ex)
            {
                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
                this.NegativeResult(ex);
            }
        }

        public async Task<MissionOperation> SuspendAsync(int id, string userName = null)
        {
            var operation = await this.missionOperationsWmsWebService.SuspendItemAsync(id, userName);
            var messageData = new MissionOperationCompletedMessageData
            {
                MissionOperationId = id,
            };

            var notificationMessage = new NotificationMessage(
                messageData,
                "Mission Operation Suspended",
                MessageActor.MissionManager,
                MessageActor.WebApi,
                MessageType.MissionOperationCompleted,
                BayNumber.None);

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(notificationMessage);

            return operation;
        }

        private void NegativeResult(WmsWebApiException exception)
        {
            ProblemDetails problemDetails;
            if (exception is WmsWebApiException<ProblemDetails> problemDetailsException)
            {
                problemDetails = problemDetailsException.Result;
            }
            else
            {
                problemDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<ProblemDetails>(exception.Response);
            }

            switch (exception.StatusCode)
            {
                case (int)HttpStatusCode.BadRequest:
                    throw new ArgumentException(problemDetails?.Detail);

                case (int)HttpStatusCode.NotFound:
                    throw new EntityNotFoundException();

                case (int)HttpStatusCode.UnprocessableEntity:
                    throw new InvalidOperationException(problemDetails?.Detail);

                default:
                    throw exception;
            }
        }

        #endregion
    }
}
