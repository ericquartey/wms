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
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IErrorsProvider errorsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<LoadingUnitsController> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMissionSchedulingProvider missionSchedulingProvider;

        private readonly IMoveLoadUnitProvider moveLoadingUnitProvider;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IMoveLoadUnitProvider moveLoadingUnitProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineProvider machineProvider,
            IMissionSchedulingProvider missionSchedulingProvider,
            IErrorsProvider errorsProvider,
            ILogger<LoadingUnitsController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.missionSchedulingProvider = missionSchedulingProvider ?? throw new ArgumentNullException(nameof(missionSchedulingProvider));
            this.moveLoadingUnitProvider = moveLoadingUnitProvider ?? throw new ArgumentNullException(nameof(moveLoadingUnitProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
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
        public IActionResult Abort(int? missionId, BayNumber targetBay)
        {
            this.logger.LogInformation($"Abort Move mission {missionId}");
            this.moveLoadingUnitProvider.AbortMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("add-test-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult AddTestUnit(DataModels.LoadingUnit loadingUnitId)
        {
            this.loadingUnitsDataProvider.AddTestUnit(loadingUnitId);
            return this.Accepted();
        }

        [HttpPost("eject-loading-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult EjectLoadingUnit(LoadingUnitLocation destination, int loadingUnitId)
        {
            if (destination == LoadingUnitLocation.Cell || destination == LoadingUnitLocation.LoadUnit)
            {
                return this.BadRequest();
            }

            this.logger.LogInformation($"Eject load unit {loadingUnitId} to destination {destination}");
            this.moveLoadingUnitProvider.EjectFromCell(MissionType.Manual, destination, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpGet]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAll()
        {
            return this.Ok(this.loadingUnitsDataProvider.GetAll());
        }

        [HttpGet("get-all-not-test-units")]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAllNotTestUnits()
        {
            return this.Ok(this.loadingUnitsDataProvider.GetAllNotTestUnits());
        }

        [HttpGet("get-all-test-units")]
        public ActionResult<IEnumerable<DataModels.LoadingUnit>> GetAllTestUnits()
        {
            return this.Ok(this.loadingUnitsDataProvider.GetAllTestUnits());
        }

        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<CompartmentDetails>>> GetCompartmentsAsync(int id, [FromServices] ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
        {
            if (loadingUnitsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsWmsWebService));
            }

            try
            {
                return this.Ok(await loadingUnitsWmsWebService.GetCompartmentsAsync(id));
            }
            catch (WmsWebApiException ex)
            {
                this.errorsProvider.RecordNew(MachineErrorCode.WmsError, BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
            }
            return this.Ok();
        }

        [HttpGet("statistics/space")]
        public async Task<ActionResult<IEnumerable<LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync(
            [FromServices] IWmsSettingsProvider wmsSettingsProvider,
            [FromServices] IMachinesWmsWebService machinesWmsWebService)
        {
            if (wmsSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(wmsSettingsProvider));
            }

            if (machinesWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(machinesWmsWebService));
            }

            var statistics = this.loadingUnitsDataProvider.GetSpaceStatistics();

            if (wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    var machineId = this.machineProvider.GetIdentity();
                    var loadingUnits = await machinesWmsWebService.GetLoadingUnitsByIdAsync(machineId);
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
            [FromServices] IWmsSettingsProvider wmsSettingsProvider,
            [FromServices] IMachinesWmsWebService machineWebService)
        {
            if (wmsSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(wmsSettingsProvider));
            }

            if (machineWebService is null)
            {
                throw new ArgumentNullException(nameof(machineWebService));
            }

            var statistics = this.loadingUnitsDataProvider.GetWeightStatistics();

            if (wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    var machineId = this.machineProvider.GetIdentity();

                    var loadingUnits = await machineWebService.GetLoadingUnitsByIdAsync(machineId);
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

        [HttpGet("{id}/wms/details")]
        public async Task<ActionResult<LoadingUnitDetails>> GetWmsDetailsByIdAsync(int id, [FromServices] ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
        {
            if (loadingUnitsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsWmsWebService));
            }

            return this.Ok(await loadingUnitsWmsWebService.GetByIdAsync(id));
        }

        [HttpPost("insert-loading-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult InsertLoadingUnit(LoadingUnitLocation source, int? destinationCellId, int loadingUnitId)
        {
            this.logger.LogInformation($"Insert load unit {loadingUnitId} from {source} to cell {destinationCellId}");
            var missionType = (source == LoadingUnitLocation.Elevator) ? MissionType.Manual : MissionType.LoadUnitOperation;
            this.moveLoadingUnitProvider.InsertToCell(missionType, source, destinationCellId, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

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
        public async Task<IActionResult> MoveToBayAsync(
            int id,
            [FromServices] IWmsSettingsProvider wmsSettingsProvider,
            [FromServices] ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
        {
            if (wmsSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(wmsSettingsProvider));
            }

            if (loadingUnitsWmsWebService is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsWmsWebService));
            }

            if (wmsSettingsProvider.IsEnabled)
            {
                try
                {
                    await loadingUnitsWmsWebService.WithdrawAsync(id, (int)this.BayNumber);
                }
                catch (Exception ex)
                {
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
                }
            }
            else
            {
                this.logger.LogInformation($"Move load unit {id} to bay {this.BayNumber}");
                try
                {
                    this.missionSchedulingProvider.QueueBayMission(id, this.BayNumber, MissionType.OUT);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(MachineErrorCode.LoadUnitNotFound, this.BayNumber, ex.Message);
                }
            }

            return this.Accepted();
        }

        [HttpGet("pause-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Pause(int? missionId, BayNumber targetBay)
        {
            this.moveLoadingUnitProvider.PauseMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("{id}/recall")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult RemoveFromBayAsync(int id)
        {
            this.logger.LogInformation($"Move load unit {id} back from bay {this.BayNumber}");
            this.missionSchedulingProvider.QueueRecallMission(id, this.BayNumber, MissionType.IN);

            return this.Accepted();
        }

        [HttpGet("remove-loadunit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RemoveLoadUnit(int loadingUnitId)
        {
            this.loadingUnitsDataProvider.Remove(loadingUnitId);
            return this.Accepted();
        }

        [HttpPost("remove-test-unit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RemoveTestUnit(DataModels.LoadingUnit loadingUnitId)
        {
            this.loadingUnitsDataProvider.RemoveTestUnit(loadingUnitId);
            return this.Accepted();
        }

        [HttpGet("resume-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Resume(int? missionId, BayNumber targetBay)
        {
            this.logger.LogInformation($"Resume mission {missionId} in bay {targetBay}");
            this.moveLoadingUnitProvider.RemoveLoadUnit(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpGet("{id}/resume-moving-wms")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ResumeWms(int id, int missionId, [FromServices] IBaysDataProvider baysDataProvider)
        {
            if (baysDataProvider is null)
            {
                throw new ArgumentNullException(nameof(baysDataProvider));
            }

            var loadingUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(id);

            this.moveLoadingUnitProvider.ResumeMoveLoadUnit(
                    missionId,
                    loadingUnitSource,
                    LoadingUnitLocation.Cell,
                    this.BayNumber,
                    null,
                    MissionType.IN,
                    MessageActor.MissionManager);

            return this.Accepted();
        }

        [HttpPost("save-loadunit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult SaveLoadUnit(DataModels.LoadingUnit loadingUnit)
        {
            this.loadingUnitsDataProvider.Save(loadingUnit);
            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToBay(int loadingUnitId, LoadingUnitLocation destination)
        {
            this.logger.LogInformation($"Move load unit {loadingUnitId} to destination {destination} bay {this.BayNumber}");
            var missionType = (destination == LoadingUnitLocation.Elevator) ? MissionType.Manual : MissionType.LoadUnitOperation;
            this.moveLoadingUnitProvider.MoveLoadUnitToBay(missionType, loadingUnitId, destination, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToCell(int loadingUnitId, int? destinationCellId)
        {
            this.logger.LogInformation($"Move load unit {loadingUnitId} to cell {destinationCellId} bay {this.BayNumber}");
            this.moveLoadingUnitProvider.MoveLoadUnitToCell(MissionType.LoadUnitOperation, loadingUnitId, destinationCellId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-source-destination")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingSourceDestination(LoadingUnitLocation source, LoadingUnitLocation destination, int? sourceCellId, int? destinationCellId)
        {
            this.logger.LogInformation($"Move from {source} cell {sourceCellId} to {destination} cell {destinationCellId} bay {this.BayNumber}");
            if (source == LoadingUnitLocation.Cell && destination == LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToCell(MissionType.LoadUnitOperation, sourceCellId, destinationCellId, this.BayNumber, MessageActor.AutomationService);
            }
            else if (source != LoadingUnitLocation.Cell && destination != LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromBayToBay(MissionType.LoadUnitOperation, source, destination, this.BayNumber, MessageActor.AutomationService);
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
        public IActionResult Stop(int? missionId, BayNumber targetBay)
        {
            this.logger.LogInformation($"Stop Move mission {missionId}");
            this.moveLoadingUnitProvider.StopMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
