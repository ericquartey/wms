using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
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

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly IMachinesDataService machinesDataService;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IEventAggregator eventAggregator,
            ILoadingUnitsProvider loadingUnitsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IMachinesDataService machinesDataService)
            : base(eventAggregator)
        {
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new System.ArgumentNullException(nameof(setupProceduresDataProvider));
            this.machinesDataService = machinesDataService ?? throw new System.ArgumentNullException(nameof(machinesDataService));
        }

        #endregion

        #region Methods

        [HttpPost("deposit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Deposit(DrawerDestination destination, double targetPosition, bool isPositive)
        {
            var parameters = this.setupProceduresDataProvider.GetAll().HorizontalManualMovements;

            var drawerOperationData = new DrawerOperationMessageData(
               DrawerOperation.Deposit,
               DrawerOperationStep.None)
            {
                Destination = destination,
                DestinationVerticalPosition = targetPosition,
                IsDestinationPositive = isPositive,
                DestinationHorizontalPosition = parameters.RecoveryTargetPosition
            };

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
                        stat.AreaFillPercentage = loadingUnit.AreaFillRate.Value * 100;
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
        public IActionResult Pickup(DrawerDestination source, double targetPosition, bool isPositive)
        {
            var parameters = this.setupProceduresDataProvider.GetAll().HorizontalManualMovements;

            var drawerOperationData = new DrawerOperationMessageData(
               DrawerOperation.Pickup,
               DrawerOperationStep.None)
            {
                Source = source,
                SourceVerticalPosition = targetPosition,
                IsSourcePositive = isPositive,
                SourceHorizontalPosition = parameters.RecoveryTargetPosition
            };

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
