using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;
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

        public CommonUtils.Messages.Enumerations.BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        // Interrupts operation finishing current movement leaving machine in a safe state
        [HttpGet("abort-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Abort(int? missionId, CommonUtils.Messages.Enumerations.BayNumber targetBay)
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
        public IActionResult EjectLoadingUnit(CommonUtils.Messages.Enumerations.LoadingUnitLocation destination, int loadingUnitId)
        {
            if (destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell || destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.LoadUnit)
            {
                return this.BadRequest();
            }

            this.logger.LogInformation($"Eject load unit {loadingUnitId} to destination {destination}");
            this.moveLoadingUnitProvider.EjectFromCell(CommonUtils.Messages.Enumerations.MissionType.Manual, destination, loadingUnitId, this.BayNumber, MessageActor.AutomationService);

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
        public async Task<ActionResult<IEnumerable<Contracts.CompartmentDetails>>> GetCompartmentsAsync(int id, [FromServices] ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
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
                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, CommonUtils.Messages.Enumerations.BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
            }
            return this.Ok();
        }

        [HttpGet("get-load-unit-max-height")]
        public ActionResult<double> GetLoadUnitMaxHeight()
        {
            return this.Ok(this.loadingUnitsDataProvider.GetLoadUnitMaxHeight());
        }

        [HttpGet("get-LU-tare")]
        public ActionResult<double> GetMachineLoadingUnitTare()
        {
            var machine = this.machineProvider.Get();
            return this.Ok(machine.LoadUnitTare);
        }

        [HttpGet("statistics/space")]
        public async Task<ActionResult<IEnumerable<DataModels.LoadingUnitSpaceStatistics>>> GetSpaceStatisticsAsync(
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
        public async Task<ActionResult<IEnumerable<DataModels.LoadingUnitWeightStatistics>>> GetWeightStatisticsAsync(
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
        public async Task<ActionResult<Contracts.LoadingUnitDetails>> GetWmsDetailsByIdAsync(int id, [FromServices] ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
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
        public IActionResult InsertLoadingUnit(CommonUtils.Messages.Enumerations.LoadingUnitLocation source, int? destinationCellId, int loadingUnitId)
        {
            this.logger.LogInformation($"Insert load unit {loadingUnitId} from {source} to cell {destinationCellId}");
            var missionType = (source == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Elevator) ? CommonUtils.Messages.Enumerations.MissionType.Manual : CommonUtils.Messages.Enumerations.MissionType.LoadUnitOperation;
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
            [FromServices] IMachineLoadingUnitsAdapterWebService loadingUnitsWmsWebService
            )
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
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, CommonUtils.Messages.Enumerations.BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
                }
            }
            else
            {
                this.logger.LogInformation($"Move load unit {id} to bay {this.BayNumber}");
                try
                {
                    this.missionSchedulingProvider.QueueBayMission(id, this.BayNumber, CommonUtils.Messages.Enumerations.MissionType.OUT);
                }
                catch (InvalidOperationException ex)
                {
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.LoadUnitNotFound, this.BayNumber, ex.Message);
                }
            }

            return this.Accepted();
        }

        [HttpGet("pause-moving")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Pause(int? missionId, CommonUtils.Messages.Enumerations.BayNumber targetBay)
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
            this.missionSchedulingProvider.QueueRecallMission(id, this.BayNumber, CommonUtils.Messages.Enumerations.MissionType.IN);

            return this.Accepted();
        }

        [HttpGet("remove-loadunit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult RemoveLoadUnit(int loadingUnitId)
        {
            this.logger.LogInformation($"Remove load unit {loadingUnitId} by UI");
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
        public IActionResult Resume(int? missionId, CommonUtils.Messages.Enumerations.BayNumber targetBay)
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
                    CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell,
                    this.BayNumber,
                    null,
                    CommonUtils.Messages.Enumerations.MissionType.IN,
                    MessageActor.MissionManager);

            return this.Accepted();
        }

        [HttpPost("save-loadunit")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult SaveLoadUnit(DataModels.LoadingUnit loadingUnit,
            [FromServices] IMachineLoadingUnitsAdapterWebService loadingUnitsWmsWebService,
            [FromServices] IWmsSettingsProvider wmsSettingsProvider)
        {
            if (loadingUnit is null)
            {
                throw new ArgumentNullException(nameof(loadingUnit));
            }

            this.loadingUnitsDataProvider.Save(loadingUnit);

            if (wmsSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(wmsSettingsProvider));
            }

            if (wmsSettingsProvider.IsEnabled)
            {
                if (loadingUnitsWmsWebService is null)
                {
                    throw new ArgumentNullException(nameof(loadingUnitsWmsWebService));
                }
                var loadUnitDetail = new Contracts.LoadingUnitDetails();
                loadUnitDetail.Id = loadingUnit.Id;
                loadUnitDetail.Height = loadingUnit.Height;
                loadUnitDetail.CellId = loadingUnit.CellId;
                loadUnitDetail.Weight = (int)loadingUnit.GrossWeight;
                loadUnitDetail.EmptyWeight = loadingUnit.Tare;
                loadUnitDetail.CellSide = Contracts.Side.NotSpecified;
                loadUnitDetail.Code = loadingUnit.Code;
                loadUnitDetail.CreationDate = DateTime.Now;

                try
                {
                    //var ret = loadingUnitsWmsWebService.UpdateAsync(loadUnitDetail, loadingUnit.Id); // non va
                    //loadingUnitsWmsWebService.BaseUrl = wmsSettingsProvider.ServiceUrl.AbsoluteUri;
                    loadingUnitsWmsWebService.SaveAsync(loadingUnit.Id, loadUnitDetail); // OK
                }
                catch (Exception ex)
                {
                    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.WmsError, CommonUtils.Messages.Enumerations.BayNumber.None, ex.Message.Replace("\n", " ").Replace("\r", " "));
                }
            }

            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToBay(int loadingUnitId, CommonUtils.Messages.Enumerations.LoadingUnitLocation destination)
        {
            this.logger.LogInformation($"Move load unit {loadingUnitId} to destination {destination} bay {this.BayNumber}");
            var missionType = (destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Elevator) ? CommonUtils.Messages.Enumerations.MissionType.Manual : CommonUtils.Messages.Enumerations.MissionType.LoadUnitOperation;
            this.moveLoadingUnitProvider.MoveLoadUnitToBay(missionType, loadingUnitId, destination, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-loading-unit-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingLoadingUnitToCell(int loadingUnitId, int? destinationCellId)
        {
            this.logger.LogInformation($"Move load unit {loadingUnitId} to cell {destinationCellId} bay {this.BayNumber}");
            this.moveLoadingUnitProvider.MoveLoadUnitToCell(CommonUtils.Messages.Enumerations.MissionType.LoadUnitOperation, loadingUnitId, destinationCellId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("start-moving-source-destination")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartMovingSourceDestination(CommonUtils.Messages.Enumerations.LoadingUnitLocation source, CommonUtils.Messages.Enumerations.LoadingUnitLocation destination, int? sourceCellId, int? destinationCellId)
        {
            this.logger.LogInformation($"Move from {source} cell {sourceCellId} to {destination} cell {destinationCellId} bay {this.BayNumber}");
            if (source == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell && destination == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToCell(CommonUtils.Messages.Enumerations.MissionType.LoadUnitOperation, sourceCellId, destinationCellId, this.BayNumber, MessageActor.AutomationService);
            }
            else if (source != CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell && destination != CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromBayToBay(CommonUtils.Messages.Enumerations.MissionType.LoadUnitOperation, source, destination, this.BayNumber, MessageActor.AutomationService);
            }
            else if (source == CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell && destination != CommonUtils.Messages.Enumerations.LoadingUnitLocation.Cell)
            {
                this.moveLoadingUnitProvider.MoveFromCellToBay(CommonUtils.Messages.Enumerations.MissionType.Manual, sourceCellId, destination, this.BayNumber, MessageActor.AutomationService);
            }
            else
            {
                this.moveLoadingUnitProvider.MoveFromBayToCell(CommonUtils.Messages.Enumerations.MissionType.Manual, source, destinationCellId, this.BayNumber, MessageActor.AutomationService);
            }

            return this.Accepted();
        }

        [HttpPost("start-scale-calibration")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult StartScaleCalibration(int loadingUnitId)
        {
            this.logger.LogInformation($"Move load unit {loadingUnitId} to destination {CommonUtils.Messages.Enumerations.LoadingUnitLocation.Elevator} bay {this.BayNumber}");
            this.moveLoadingUnitProvider.MoveLoadUnitToBay(CommonUtils.Messages.Enumerations.MissionType.ScaleCalibration, loadingUnitId, CommonUtils.Messages.Enumerations.LoadingUnitLocation.Elevator, this.BayNumber, MessageActor.AutomationService);

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
        public IActionResult Stop(int? missionId, CommonUtils.Messages.Enumerations.BayNumber targetBay)
        {
            this.logger.LogInformation($"Stop Move mission {missionId}");
            this.moveLoadingUnitProvider.StopMove(missionId, this.BayNumber, targetBay, MessageActor.AutomationService);
            return this.Accepted();
        }

        #endregion
    }
}
