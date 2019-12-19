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

        private readonly IMissionOperationsDataService missionOperationsDataService;

        private readonly IMissionsDataProvider missionsDataProvider;

        #endregion

        #region Constructors

        public MissionOperationsProvider(
            IEventAggregator eventAggregator,
            IMissionOperationsDataService missionOperationsDataService,
            IMissionsDataProvider missionsDataProvider,
            IConfiguration configuration)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
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
                await this.missionOperationsDataService.AbortAsync(wmsId);
            }
            catch (SwaggerException ex)
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
        public async Task CompleteAsync(int wmsId, double quantity)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsDataService.CompleteItemAsync(wmsId, quantity);

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
            catch (SwaggerException ex)
            {
                this.NegativeResult(ex);
            }
        }

        public async Task<MissionOperation> GetByIdAsync(int wmsId)
        {
            return await this.missionOperationsDataService.GetByIdAsync(wmsId);
        }

        public int GetCountByBay(BayNumber bayNumber)
        {
            return this.missionsDataProvider
                .GetAllActiveMissionsByBay(bayNumber)
                .Where(m => m.WmsId != null && m.Status != CommonUtils.Messages.Enumerations.MissionStatus.Completed)
                .Count();
        }

        /// <summary>
        /// the UI informs mission manager that the operation is completed
        /// </summary>
        /// <param name="id">operation id</param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public async Task PartiallyCompleteAsync(int wmsId, double quantity)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            try
            {
                await this.missionOperationsDataService.PartiallyCompleteAndRescheduleItemAsync(wmsId, quantity);

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
            catch (SwaggerException ex)
            {
                this.NegativeResult(ex);
            }
        }

        private void NegativeResult(SwaggerException exception)
        {
            var problemDetails = new ProblemDetails();
            if (exception is SwaggerException<ProblemDetails> problemDetailsException)
            {
                problemDetails = problemDetailsException.Result;
            }

            switch (exception.StatusCode)
            {
                case (int)HttpStatusCode.BadRequest:
                    throw new ArgumentException(problemDetails.Detail);

                case (int)HttpStatusCode.NotFound:
                    throw new EntityNotFoundException();

                case (int)HttpStatusCode.UnprocessableEntity:
                    throw new InvalidOperationException(problemDetails.Detail);

                default:
                    throw exception;
            }
        }

        #endregion
    }
}
