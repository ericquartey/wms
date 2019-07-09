using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Operator/[controller]")]
    [ApiController]
    public class OperatorController : ControllerBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IItemsDataService itemsDataService;

        private readonly ILogger<AutomationService> logger;

        private readonly IMissionsDataService missionsDataService;

        private readonly IServiceProvider services;

        #endregion

        #region Constructors

        public OperatorController(IEventAggregator eventAggregator, ILogger<AutomationService> logger, IServiceProvider services, IItemsDataService itemsDataService, IMissionsDataService missionsDataService)
        {
            this.eventAggregator = eventAggregator;
            this.services = services;
            this.itemsDataService = itemsDataService;
            this.missionsDataService = missionsDataService;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpGet("Pick/{bayId}/{missionId}/{evadedQuantity}")]
        public async void PickAsync(int bayId, int missionId, int evadedQuantity)
        {
            try
            {
                await this.missionsDataService.CompleteItemAsync(missionId, evadedQuantity);
                var messageData = new MissionCompletedMessageData
                {
                    MissionId = missionId,
                    BayId = bayId,
                };
                var notificationMessage = new NotificationMessage(messageData, "Mission Completed", MessageActor.MissionsManager, MessageActor.WebApi, MessageType.MissionCompleted, MessageStatus.NoStatus);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                this.logger.LogDebug($"AS-OC Received HTTP Get request from bay {bayId}, mission Id {missionId}, evaded quantity {evadedQuantity}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }

        [HttpGet("Refill/{bayId}/{missionId}/{evadedQuantity}")]
        public async void RefillAsync(int bayId, int missionId, int evadedQuantity)
        {
            try
            {
                await this.missionsDataService.CompleteItemAsync(missionId, evadedQuantity);
                var messageData = new MissionCompletedMessageData
                {
                    MissionId = missionId,
                    BayId = bayId,
                };
                var notificationMessage = new NotificationMessage(messageData, "Mission Completed", MessageActor.MissionsManager, MessageActor.WebApi, MessageType.MissionCompleted, MessageStatus.NoStatus);
                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                this.logger.LogDebug($"AS-OC Received HTTP Get request from bay {bayId}, mission Id {missionId}, evaded quantity {evadedQuantity}");
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message);
            }
        }

        #endregion
    }
}
