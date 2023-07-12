using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MachineModeProvider : BaseProvider, IMachineModeProvider
    {
        #region Fields

        private readonly ILogger<MachineModeProvider> logger;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public MachineModeProvider(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILogger<MachineModeProvider> logger,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public MachineMode GetCurrent()
        {
            return this.machineVolatileDataProvider.UiFilteredMode;
        }

        public void RequestChange(
            MachineMode machineMode,
            BayNumber bayNumber = BayNumber.None,
            List<int> loadUnits = null,
            int? cycles = null,
            bool randomCells = false,
            bool optimizeRotationClass = false)
        {
            this.machineVolatileDataProvider.RandomCells = randomCells;

            if (machineMode == this.machineVolatileDataProvider.Mode)
            {
                return;
            }

            switch (machineMode)
            {
                case MachineMode.Automatic:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToAutomatic;
                    break;

                case MachineMode.Manual:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToManual;
                    break;

                case MachineMode.Manual2:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToManual2;
                    break;

                case MachineMode.Manual3:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToManual3;
                    break;

                case MachineMode.Compact:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.Compact2:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact2;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.Compact3:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact3;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.FastCompact:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFastCompact;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.FastCompact2:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFastCompact;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.FastCompact3:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFastCompact;
                    this.machineVolatileDataProvider.IsOptimizeRotationClass = optimizeRotationClass;
                    break;

                case MachineMode.FullTest:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.RequiredCycles = cycles;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.LoadUnitsExecutedCycles = loadUnits.ToDictionary(key => key, value => 0);

                    var procedureParameters = this.setupProceduresDataProvider.GetFullTest(bayNumber);
                    this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters);

                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.FullTest2:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.RequiredCycles = cycles;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.LoadUnitsExecutedCycles = loadUnits.ToDictionary(key => key, value => 0);

                    var procedureParameters2 = this.setupProceduresDataProvider.GetFullTest(bayNumber);
                    this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters2);

                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest2;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.FullTest3:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.RequiredCycles = cycles;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.LoadUnitsExecutedCycles = loadUnits.ToDictionary(key => key, value => 0);

                    var procedureParameters3 = this.setupProceduresDataProvider.GetFullTest(bayNumber);
                    this.setupProceduresDataProvider.ResetPerformedCycles(procedureParameters3);

                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest3;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.FirstTest:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.FirstTest2:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest2;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.FirstTest3:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.ExecutedCycles = 0;
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest3;
                    this.machineVolatileDataProvider.StopTest = false;
                    break;

                case MachineMode.Shutdown:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToShutdown;
                    break;

                default:
                    throw new ArgumentException($"The requested machine mode '{machineMode}' cannot be handled.", nameof(machineMode));
            }

            this.logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");

            this.SendCommandToMachineManager(
                new MachineModeMessageData(machineMode),
                $"Request mode change to '{machineMode}'",
                MessageActor.MissionManager,
                MessageType.MachineMode,
                BayNumber.All);
        }

        public void StopTest()
        {
            this.machineVolatileDataProvider.StopTest = true;
        }

        #endregion
    }
}
