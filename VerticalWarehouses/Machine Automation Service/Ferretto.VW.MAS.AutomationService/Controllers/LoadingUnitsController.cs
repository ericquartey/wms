using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MissionsManager.Providers.Interfaces;
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

        private readonly IMoveLoadingUnitProvider moveLoadingUnitProvider;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IEventAggregator eventAggregator,
            IMoveLoadingUnitProvider moveLoadingUnitProvider,
            ILoadingUnitsProvider loadingUnitsProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IMachinesDataService machinesDataService)
            : base(eventAggregator)
        {
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new System.ArgumentNullException(nameof(loadingUnitsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new System.ArgumentNullException(nameof(setupProceduresDataProvider));
            this.machinesDataService = machinesDataService ?? throw new System.ArgumentNullException(nameof(machinesDataService));
            this.moveLoadingUnitProvider = moveLoadingUnitProvider ?? throw new ArgumentNullException(nameof(moveLoadingUnitProvider));
            }

        #endregion

        #region Methods

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
                        if (loadingUnit.AreaFillRate != null)
                        {
                            stat.AreaFillPercentage = loadingUnit.AreaFillRate.Value * 100;
                        }
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

        [HttpPost("start-moving-loading-unit-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToBay(int loadingUnitId, LoadingUnitDestination destination)
        {
            if (destination == LoadingUnitDestination.Cell)
            {
                return this.BadRequest();
            }

            this.moveLoadingUnitProvider.MoveLoadingUnitToBay(loadingUnitId, destination, this.BayNumber);

            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToCell(int loadingUnitId, int destinationCellId)
        {
            this.moveLoadingUnitProvider.MoveLoadingUnitToCell(loadingUnitId, destinationCellId, this.BayNumber);

            return this.Accepted();
        }

        [HttpPost("start-moving-source-destination")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingSourceDestination(LoadingUnitDestination source, LoadingUnitDestination destination, int sourceCellId, int destinationCellId)
        {
            if (source == LoadingUnitDestination.Cell && destination == LoadingUnitDestination.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToCell(sourceCellId, destinationCellId, this.BayNumber);
            }
            else if (source != LoadingUnitDestination.Cell && destination != LoadingUnitDestination.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromBayToBay(source, destination, this.BayNumber);
            }
            else if (source == LoadingUnitDestination.Cell && destination != LoadingUnitDestination.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToBay(sourceCellId, destination, this.BayNumber);
            }
            else
            {
                this.moveLoadingUnitProvider.MoveFromBayToCell(source, destinationCellId, this.BayNumber);
            }

            return this.Accepted();
        }

        [HttpGet("stop-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.moveLoadingUnitProvider.StopMoving();
            return this.Accepted();
        }

        #endregion
    }
}
