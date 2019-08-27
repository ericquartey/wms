using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionOperationsController : BaseWmsProxyBaseController
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        #endregion

        #region Constructors

        public MissionOperationsController(
            IEventAggregator eventAggregator,
            ILogger<AutomationService> logger,
            IMissionOperationsDataService missionOperationsDataService)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (logger is null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (missionOperationsDataService is null)
            {
                throw new ArgumentNullException(nameof(missionOperationsDataService));
            }

            this.eventAggregator = eventAggregator;
            this.logger = logger;
            this.missionOperationsDataService = missionOperationsDataService;
        }

        #endregion

        #region Methods

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity)
        {
            try
            {
                await this.missionOperationsDataService.CompleteItemAsync(id, quantity);

                var messageData = new MissionOperationCompletedMessageData
                {
                    MissionId = id,
                };

                var notificationMessage = new NotificationMessage(
                    messageData,
                    "Mission Operation Completed",
                    MessageActor.MissionsManager,
                    MessageActor.WebApi,
                    MessageType.MissionOperationCompleted);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                this.logger.LogDebug($"AS-OC Operator marked mission operation id={id} as completed, with quantity {quantity}.");

                return this.Ok();
            }
            catch (SwaggerException ex)
            {
                return this.NegativeResult(ex);
            }
        }

        #endregion
    }
}
