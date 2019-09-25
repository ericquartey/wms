using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController : BaseAutomationController
    {

        #region Fields

        private readonly IHorizontalManualMovementsDataLayer horizontalManualMovements;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachinesDataService machinesDataService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IEventAggregator eventAggregator,
            ILoadingUnitsProvider loadingUnitStatisticsProvider,
            IHorizontalManualMovementsDataLayer horizontalManualMovementsDataLayer,
            IMachinesDataService machinesDataService)
            : base(eventAggregator)
        {
            if (loadingUnitStatisticsProvider is null)
            {
                throw new System.ArgumentNullException(nameof(loadingUnitStatisticsProvider));
            }

            if (machinesDataService is null)
            {
                throw new System.ArgumentNullException(nameof(machinesDataService));
            }

            if (horizontalManualMovementsDataLayer is null)
            {
                throw new System.ArgumentNullException(nameof(horizontalManualMovementsDataLayer));
            }

            this.loadingUnitsProvider = loadingUnitStatisticsProvider;
            this.machinesDataService = machinesDataService;
            this.horizontalManualMovements = horizontalManualMovementsDataLayer;
        }

        #endregion



        #region Methods

        [HttpPost("deposit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Deposit(DrawerDestination destination, decimal targetPosition, bool isPositive)
        {
            var drawerOperationData = new DrawerOperationMessageData(
               DrawerOperation.Deposit,
               DrawerOperationStep.None);

            drawerOperationData.Destination = destination;
            drawerOperationData.DestinationVerticalPosition = targetPosition;
            drawerOperationData.IsDestinationPositive = isPositive;
            drawerOperationData.DestinationHorizontalPosition = this.horizontalManualMovements.RecoveryTargetPositionHM;
            if (!isPositive)
            {
                drawerOperationData.DestinationHorizontalPosition *= -1;
            }

            this.PublishCommand(
                drawerOperationData,
                "Execute Drawer Operation Command",
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation);

            return this.Accepted();
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAll()
        {
            var loadingUnits = this.loadingUnitsProvider.GetAll();
            return this.Ok(loadingUnits);
        }

        [HttpGet("statistics/space")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync()
        {
            var statistics = this.loadingUnitsProvider.GetSpaceStatistics();

            try
            {
                var machineId = 1; // TODO HACK remove this hardcoded value
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                        stat.AreaFillPercentage = (decimal?)loadingUnit.AreaFillRate.Value * 100;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing:
                // data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        [HttpGet("statistics/weight")]
        public async Task<ActionResult<IEnumerable<LoadingUnitWeightStatistics>>> GetWeightStatisticsAsync()
        {
            var statistics = this.loadingUnitsProvider.GetWeightStatistics();
            try
            {
                var machineId = 1; // TODO HACK remove this hardcoded value
                var loadingUnits = await this.machinesDataService.GetLoadingUnitsByIdAsync(machineId);
                foreach (var stat in statistics)
                {
                    var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                    if (loadingUnit != null)
                    {
                        stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                    }
                }
            }
            catch (System.Exception)
            {
                // do nothing:
                // data from WMS will remain to its default values
            }

            return this.Ok(statistics);
        }

        [HttpPost("pickup")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Pickup(DrawerDestination source, decimal targetPosition, bool isPositive)
        {
            var drawerOperationData = new DrawerOperationMessageData(
               DrawerOperation.Pickup,
               DrawerOperationStep.None);

            drawerOperationData.Source = source;
            drawerOperationData.SourceVerticalPosition = targetPosition;
            drawerOperationData.IsSourcePositive = isPositive;
            drawerOperationData.SourceHorizontalPosition = this.horizontalManualMovements.RecoveryTargetPositionHM;
            if (!isPositive)
            {
                drawerOperationData.SourceHorizontalPosition *= -1;
            }

            this.PublishCommand(
                drawerOperationData,
                "Execute Drawer Operation Command",
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation);

            return this.Accepted();
        }

        [HttpPost("start-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMoving(DrawerOperation drawerOperation)
        {
            var drawerOperationData = new DrawerOperationMessageData(
               drawerOperation,
               DrawerOperationStep.None);

            drawerOperationData.Source = DrawerDestination.InternalBay1Up; // TODO HACK remove this hardcoded value
            drawerOperationData.Destination = DrawerDestination.Cell;

            this.PublishCommand(
                drawerOperationData,
                "Execute Drawer Operation Command",
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation);

            return this.Accepted();
        }

        [HttpGet("stop-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            var messageData = new StopMessageData(StopRequestReason.Stop);
            this.PublishCommand(
                messageData,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
        }

        #endregion
    }
}
