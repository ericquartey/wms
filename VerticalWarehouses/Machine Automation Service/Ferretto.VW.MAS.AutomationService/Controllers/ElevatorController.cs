﻿using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Ferretto.VW.MAS.AutomationService;
using Ferretto.VW.MAS.DeviceManager;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElevatorController : ControllerBase, IRequestingBayController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ElevatorController(
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            IElevatorWeightCheckProcedureProvider elevatorWeightCheckProvider,
            IEventAggregator eventAggregator)
        {
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.elevatorWeightCheckProvider = elevatorWeightCheckProvider ?? throw new ArgumentNullException(nameof(elevatorWeightCheckProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        #endregion

        #region Methods

        [HttpGet("horizontal/can-extract-from-bay/{bayPositionId}")]
        public ActionResult<ActionPolicy> CanExtractFromBay(int bayPositionId)
        {
            return this.Ok(this.elevatorProvider.CanExtractFromBay(bayPositionId, this.BayNumber));
        }

        [HttpGet("horizontal/can-load-from-bay/{bayPositionId}")]
        public ActionResult<ActionPolicy> CanLoadFromBay(int bayPositionId, bool isGuided)
        {
            return this.Ok(this.elevatorProvider.CanLoadFromBay(bayPositionId, this.BayNumber, isGuided));
        }

        [HttpGet("horizontal/can-load-from-cell/{cellId}")]
        public ActionResult<ActionPolicy> CanLoadFromCell(int cellId)
        {
            return this.Ok(this.elevatorProvider.CanLoadFromCell(cellId, this.BayNumber));
        }

        [HttpGet("vertical/can-move-to-bay-position")]
        public ActionResult<ActionPolicy> CanMoveToBayPosition(int bayPositionId)
        {
            return this.Ok(this.elevatorProvider.CanMoveToBayPosition(bayPositionId, this.BayNumber));
        }

        [HttpGet("vertical/can-move-to-cell")]
        public ActionResult<ActionPolicy> CanMoveToCell(int cellId)
        {
            return this.Ok(this.elevatorProvider.CanMoveToCell(cellId));
        }

        [HttpGet("horizontal/can-unload-to-bay/{bayPositionId}")]
        public ActionResult<ActionPolicy> CanUnloadToBay(int bayPositionId, bool isGuided)
        {
            return this.Ok(this.elevatorProvider.CanUnloadToBay(bayPositionId, this.BayNumber, isGuided));
        }

        [HttpGet("horizontal/can-unload-to-cell/{cellId}")]
        public ActionResult<ActionPolicy> CanUnloadToCell(int cellId)
        {
            return this.Ok(this.elevatorProvider.CanUnloadToCell(cellId));
        }

        [HttpPost("horizontal/find-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult FindZero()
        {
            this.elevatorProvider.Homing(Axis.Horizontal, Calibration.FindSensor, null, true, this.BayNumber, MessageActor.WebApi);
            return this.Accepted();
        }

        [HttpGet("horizontal/assisted-movements-parameters")]
        public ActionResult<ElevatorAxisManualParameters> GetHorizontalAssistedMovementsParameters()
        {
            return this.Ok(this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Horizontal));
        }

        [HttpGet("horizontal/manual-movements-parameters")]
        public ActionResult<ElevatorAxisManualParameters> GetHorizontalManualMovementsParameters()
        {
            return this.Ok(this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Horizontal));
        }

        [HttpGet("loading-unit-on-board")]
        public ActionResult<LoadingUnit> GetLoadingUnitOnBoard()
        {
            return this.Ok(this.elevatorDataProvider.GetLoadingUnitOnBoard());
        }

        [HttpGet("position")]
        public ActionResult<ElevatorPosition> GetPosition()
        {
            var pos = this.elevatorDataProvider.GetCurrentBayPosition();
            var elevatorPosition = new ElevatorPosition
            {
                Vertical = this.elevatorProvider.VerticalPosition,
                Horizontal = this.elevatorProvider.HorizontalPosition,
                CellId = this.elevatorDataProvider.GetCurrentCell()?.Id,
                BayPositionId = pos?.Id,
                BayPositionUpper = pos?.IsUpper
            };

            return this.Ok(elevatorPosition);
        }

        [HttpGet("vertical/assisted-movements-parameters")]
        public ActionResult<ElevatorAxisManualParameters> GetVerticalAssistedMovementsParameters()
        {
            return this.Ok(this.elevatorDataProvider.GetAssistedMovementsAxis(Orientation.Vertical));
        }

        [HttpGet("vertical/bounds")]
        public ActionResult<AxisBounds> GetVerticalBounds()
        {
            return this.Ok(this.elevatorProvider.GetVerticalBounds());
        }

        [HttpGet("vertical/manual-movements-parameters")]
        public ActionResult<ElevatorAxisManualParameters> GetVerticalManualMovementsParameters()
        {
            return this.Ok(this.elevatorDataProvider.GetManualMovementsAxis(Orientation.Vertical));
        }

        [HttpGet("vertical/offset")]
        public ActionResult<double> GetVerticalOffset()
        {
            return this.Ok(this.elevatorDataProvider.GetAxis(Orientation.Vertical).Offset);
        }

        [HttpGet("vertical/resolution")]
        public ActionResult<decimal> GetVerticalResolution()
        {
            return this.Ok(this.elevatorDataProvider.GetAxis(Orientation.Vertical).Resolution);
        }

        [HttpPost("horizontal/load-from-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult LoadFromBay(int bayPositionId)
        {
            this.elevatorProvider.LoadFromBay(bayPositionId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("horizontal/load-from-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult LoadFromCell(int cellId)
        {
            this.elevatorProvider.LoadFromCell(cellId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("horizontal/move-manual")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveHorizontalManual(HorizontalMovementDirection direction)
        {
            this.elevatorProvider.MoveHorizontalManual(direction, -1, false, null, null, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("vertical/manual-move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveManualToVerticalPosition(
            double targetPosition,
            bool performWeighting,
            bool computeElongation)
        {
            this.elevatorProvider.MoveToAbsoluteVerticalPosition(
                manualMovment: true,
                targetPosition,
                computeElongation,
                performWeighting,
                targetBayPositionId: null,
                targetCellId: null,
                checkHomingDone: true,
                waitContinue: false,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/move-to-bay-position")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToBayPosition(int bayPositionId, bool computeElongation, bool performWeighting)
        {
            this.elevatorProvider.MoveToBayPosition(
                bayPositionId,
                computeElongation,
                performWeighting,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/move-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToCell(int cellId, bool computeElongation, bool performWeighting)
        {
            this.elevatorProvider.MoveToCell(
                cellId,
                computeElongation,
                performWeighting,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/move-to-free-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToFreeCell(int loadUnitId, bool computeElongation, bool performWeighting)
        {
            this.elevatorProvider.MoveToFreeCell(
                loadUnitId,
                computeElongation,
                performWeighting,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/move-to")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToVerticalPosition(
            double targetPosition,
            bool performWeighting,
            bool computeElongation)
        {
            this.elevatorProvider.MoveToAbsoluteVerticalPosition(
                manualMovment: false,
                targetPosition,
                computeElongation,
                performWeighting,
                targetBayPositionId: null,
                targetCellId: null,
                checkHomingDone: true,
                waitContinue: false,
                this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/manual-move")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalManual(VerticalMovementDirection direction)
        {
            this.elevatorProvider.MoveVerticalManual(direction, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("vertical/move-relative")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveVerticalOfDistance(double distance)
        {
            this.elevatorProvider.MoveToRelativeVerticalPosition(distance, this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("search-horizontal-zero")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult SearchHorizontalZero()
        {
            this.elevatorProvider.Homing(Axis.Horizontal, Calibration.FindSensor, null, true, this.BayNumber, MessageActor.WebApi);
            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.AutomationService);
            return this.Accepted();
        }

        [HttpPost("weight-check-stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult StopWeightCheck()
        {
            this.elevatorWeightCheckProvider.Stop();
            return this.Accepted();
        }

        [HttpPost("horizontal/unload-to-bay")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UnloadToBay(int bayPositionId)
        {
            this.elevatorProvider.UnloadToBay(bayPositionId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("horizontal/unload-to-cell")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult UnloadToCell(int cellId)
        {
            this.elevatorProvider.UnloadToCell(cellId, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("vertical/resolution")]
        public IActionResult UpdateVerticalResolution(decimal newResolution)
        {
            this.elevatorDataProvider.UpdateVerticalResolution(newResolution);

            return this.Ok();
        }

        [HttpPost("weight-check")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult WeightCheck(int loadingUnitId, double runToTest, double weight)
        {
            this.elevatorWeightCheckProvider.Start(loadingUnitId, runToTest, weight);

            return this.Accepted();
        }

        [Obsolete("Move message publishing to providers.")]
        protected void PublishCommand(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageType messageType)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        receiver,
                        MessageActor.WebApi,
                        messageType,
                        this.BayNumber));
        }

        #endregion
    }
}
