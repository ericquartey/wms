using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.Extensions.Configuration;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionOperationsProvider : IMissionOperationsProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IEventAggregator eventAggregator;

        private readonly IMissionOperationsWmsWebService missionOperationsWmsWebService;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionOperationsProvider(
            IEventAggregator eventAggregator,
            IMissionOperationsWmsWebService missionOperationsWmsWebService,
            IMissionsDataProvider missionsDataProvider,
            IConfiguration configuration)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionOperationsWmsWebService = missionOperationsWmsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWmsWebService));
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        public async Task AbortAsync(int wmsId)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsWmsWebService.AbortAsync(wmsId);
            }
            catch (WmsWebApiException ex)
            {
                this.NegativeResult(ex);
            }
        }

        /// <summary>
        /// the UI informs mission manager that the operation is completed
        /// </summary>
        /// <param name="id">operation id</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task CompleteAsync(int wmsId, double quantity, string printerName)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsWmsWebService.CompleteItemAsync(wmsId, quantity, printerName);

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

        public int GetCountByBay(BayNumber bayNumber)
        {
            return this.missionsDataProvider
                .GetAllActiveMissionsByBay(bayNumber)
                .Where(m => m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Completed
                    && m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Aborted
                    )
                .Count();
        }

        /// <summary>
        /// the UI informs mission manager that the operation is completed
        /// </summary>
        /// <param name="id">operation id</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task PartiallyCompleteAsync(int wmsId, double quantity, string printerName)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsWmsWebService.PartiallyCompleteAndRescheduleItemAsync(wmsId, quantity, printerName);

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
                this.NegativeResult(ex);
            }
        }

        private void NegativeResult(WmsWebApiException exception)
        {
            var problemDetails = new ProblemDetails();
            if (exception is WmsWebApiException<ProblemDetails> problemDetailsException)
            {
                problemDetails = problemDetailsException.Result;
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
