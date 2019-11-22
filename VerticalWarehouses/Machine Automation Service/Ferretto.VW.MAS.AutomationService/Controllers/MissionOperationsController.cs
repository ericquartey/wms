using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionOperationsController : BaseWmsProxyController
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IConfiguration configuration;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        private readonly IMissionOperationsDataService missionOperationsDataService;

        #endregion

        #region Constructors

        public MissionOperationsController(
            IEventAggregator eventAggregator,
            ILogger<MissionOperationsController> logger,
            IConfiguration configuration,
            IMissionOperationsDataService missionOperationsDataService,
            IBaysProvider baysProvider,
            IElevatorDataProvider elevatorDataProvider)
        {
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.missionOperationsDataService = missionOperationsDataService ?? throw new ArgumentNullException(nameof(missionOperationsDataService));
        }

        #endregion

        #region Methods

        [HttpPost("{id}/abort")]
        public Task<ActionResult> Abort(int id)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

            throw new NotImplementedException();
        }

        [HttpPost("{id}/complete")]
        public async Task<ActionResult> CompleteAsync(int id, double quantity)
        {
            if (!this.configuration.IsWmsEnabled())
            {
                throw new InvalidOperationException("The machine is not configured to communicate with WMS.");
            }

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
                    MessageActor.MachineManager,
                    MessageActor.WebApi,
                    MessageType.MissionOperationCompleted,
                    BayNumber.None);

                this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
                this.logger.LogDebug($"AS-OC Operator marked mission operation id={id} as completed, with quantity {quantity}.");

                return this.Ok();
            }
            catch (SwaggerException ex)
            {
                return this.NegativeResult(ex);
            }
        }

        [HttpPost("reset-machine")]
        [Obsolete("Vorrei capire se e' il punto giusto dove mettere il metodo, per oggi la lascio qui")]
        public ActionResult ResetMachine()
        {
            try
            {
                this.baysProvider.ResetMachine();
                this.elevatorDataProvider.ResetMachine();

                this.logger.LogInformation($"RESET MACHINE.");

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
