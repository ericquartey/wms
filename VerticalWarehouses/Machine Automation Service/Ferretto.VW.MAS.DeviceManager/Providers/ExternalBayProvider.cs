using System;
using System.ComponentModel;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    internal class ExternalBayProvider : BaseProvider, IExternalBayProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ExternalBayProvider> logger;

        private readonly IMachineResourcesProvider machineResourcesProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ExternalBayProvider(
            IBaysDataProvider baysDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMachineResourcesProvider machineResourcesProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator,
            ILogger<ExternalBayProvider> logger)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineResourcesProvider = machineResourcesProvider ?? throw new ArgumentNullException(nameof(machineResourcesProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public MachineErrorCode CanElevatorDeposit(BayPosition bayPosition)
        {
            return MachineErrorCode.NoError;
        }

        public MachineErrorCode CanElevatorPickup(BayPosition bayPosition)
        {
            return MachineErrorCode.NoError;
        }

        public ActionPolicy CanMove(ExternalBayMovementDirection direction, BayNumber bayNumber, MovementCategory movementCategory)
        {
            return ActionPolicy.Allowed;
        }

        public double GetPosition(BayNumber bayNumber)
        {
            return 0.0d;
        }

        public void Homing(Calibration calibration, int? loadingUnitId, bool showErrors, BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public bool IsExternalPositionOccupied(BayNumber bayNumber)
        {
            return true;
        }

        public bool IsInternalPositionOccupied(BayNumber bayNumber)
        {
            return true;
        }

        public void Move(ExternalBayMovementDirection direction, int? loadUnitId, BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void MoveAssisted(ExternalBayMovementDirection direction, BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void MoveManual(ExternalBayMovementDirection direction, double distance, int? loadUnitId, bool bypassConditions, BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void MovementForExtraction(double distance, int? loadUnitId, BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void MovementForInsertion(BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void StartTest(BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void Stop(BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        public void StopTest(BayNumber bayNumber, MessageActor sender)
        {
            return;
        }

        #endregion
    }
}
