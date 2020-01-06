using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.MissionManager;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly IMachineProvider machineProvider;

        private readonly IMachinesWmsWebService machinesWmsWebService;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMoveLoadingUnitProvider moveLoadingUnitProvider;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IMoveLoadingUnitProvider moveLoadingUnitProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineProvider machineProvider,
            IMissionSchedulingProvider missionSchedulingProvider,
            IMachinesWmsWebService machinesWmsWebService)
        {
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
            this.machinesWmsWebService = machinesWmsWebService ?? throw new ArgumentNullException(nameof(machinesWmsWebService));
            this.moveLoadingUnitProvider = moveLoadingUnitProvider ?? throw new ArgumentNullException(nameof(moveLoadingUnitProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        // Interrupts operation finishing current movement leaving machine in a safe state
        [HttpGet("abort-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Abort(Guid? missionId, BayNumber targetBay)
        {
            this.moveLoadingUnitProvider.AbortMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("eject-loading-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult EjectLoadingUnit(LoadingUnitLocation destination, int loadingUnitId)
        {
            if (destination == LoadingUnitLocation.Cell || destination == LoadingUnitLocation.LoadingUnit)
            {
                return this.BadRequest();
            }

            this.moveLoadingUnitProvider.EjectFromCell(MissionType.Manual, destination, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAll()
        {
            return this.Ok(this.loadingUnitsDataProvider.GetAll());
        }

        [HttpGet("statistics/space")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync(
            [FromServices] IConfiguration configuration)
        {
            var statistics = this.loadingUnitsDataProvider.GetSpaceStatistics();

            if (configuration.IsWmsEnabled())
            {
                try
                {
                    var machineId = this.machineProvider.GetIdentity();
                    var loadingUnits = await this.machinesWmsWebService.GetLoadingUnitsByIdAsync(machineId);
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
                catch (Exception)
                {
                    // do nothing:
                    // data from WMS will remain to its default values
                }
            }

            return this.Ok(statistics);
        }

        [HttpGet("statistics/weight")]
        public async Task<ActionResult<IEnumerable<LoadingUnitWeightStatistics>>> GetWeightStatisticsAsync(
            [FromServices] IConfiguration configuration)
        {
            var statistics = this.loadingUnitsDataProvider.GetWeightStatistics();

            if (configuration.IsWmsEnabled())
            {
                try
                {
                    var machineId = this.machineProvider.GetIdentity();
                    var loadingUnits = await this.machinesWmsWebService.GetLoadingUnitsByIdAsync(machineId);
                    foreach (var stat in statistics)
                    {
                        var loadingUnit = loadingUnits.SingleOrDefault(l => l.Code == stat.Code);
                        if (loadingUnit != null)
                        {
                            stat.CompartmentsCount = loadingUnit.CompartmentsCount;
                        }
                    }
                }
                catch (Exception)
                {
                    // do nothing:
                    // data from WMS will remain to its default values
                }
            }

            return this.Ok(statistics);
        }

        [HttpPost("insert-loading-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult InsertLoadingUnit(LoadingUnitLocation source, int? destinationCellId, int loadingUnitId)
        {
            this.moveLoadingUnitProvider.InsertToCell(MissionType.Manual, source, destinationCellId, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("insert-loading-unit-db")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public IActionResult InsertLoadingUnitOnlyDb(int loadingUnitId)
        {
            this.loadingUnitsDataProvider.Insert(loadingUnitId);
            return this.Accepted();
        }

        [HttpPost("{id}/move-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToBay(int id)
        {
            this.missionSchedulingProvider.QueueBayMission(id, this.BayNumber);

            return this.Accepted();
        }

        [HttpGet("pause-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Pause(Guid? missionId, BayNumber targetBay)
        {
            this.moveLoadingUnitProvider.PauseMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("{id}/recall")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Recall(int id)
        {
            this.missionSchedulingProvider.QueueRecallMission(id, this.BayNumber);

            return this.Accepted();
        }

        [HttpGet("resume-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Resume(Guid? missionId, BayNumber targetBay)
        {
            this.moveLoadingUnitProvider.RemoveLoadUnit(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToBay(int loadingUnitId, LoadingUnitLocation destination)
        {
            this.moveLoadingUnitProvider.MoveLoadingUnitToBay(MissionType.Manual, loadingUnitId, destination, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToCell(int loadingUnitId, int destinationCellId)
        {
            this.moveLoadingUnitProvider.MoveLoadingUnitToCell(MissionType.Manual, loadingUnitId, destinationCellId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-source-destination")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingSourceDestination(LoadingUnitLocation source, LoadingUnitLocation destination, int? sourceCellId, int? destinationCellId)
        {
            if (source == LoadingUnitLocation.Cell && destination == LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToCell(MissionType.Manual, sourceCellId, destinationCellId, this.BayNumber, MessageActor.AutomationService);
            }
            else if (source != LoadingUnitLocation.Cell && destination != LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromBayToBay(MissionType.Manual, source, destination, this.BayNumber, MessageActor.AutomationService);
            }
            else if (source == LoadingUnitLocation.Cell && destination != LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToBay(MissionType.Manual, sourceCellId, destination, this.BayNumber, MessageActor.AutomationService);
            }
            else
            {
                this.moveLoadingUnitProvider.MoveFromBayToCell(MissionType.Manual, source, destinationCellId, this.BayNumber, MessageActor.AutomationService);
            }

            return this.Accepted();
        }

        /// <summary>
        ///  Instantly stops the current operation leaving machine in a potentially unsafe condition.
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="targetBay"></param>
        /// <returns></returns>
        [HttpGet("stop-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop(Guid? missionId, BayNumber targetBay)
        {
            this.moveLoadingUnitProvider.StopMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
