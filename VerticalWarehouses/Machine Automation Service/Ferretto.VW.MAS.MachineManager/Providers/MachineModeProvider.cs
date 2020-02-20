using System;
using System.Collections.Generic;
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

        #endregion

        #region Constructors

        public MachineModeProvider(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ILogger<MachineModeProvider> logger,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public MachineMode GetCurrent()
        {
            return this.machineVolatileDataProvider.Mode;
        }

        public void RequestChange(MachineMode machineMode, BayNumber bayNumber = BayNumber.None, List<int> loadUnits = null, int? cycles = null)
        {
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

                case MachineMode.Compact:
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToCompact;
                    break;

                case MachineMode.FullTest:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.CyclesToTest = cycles;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.CyclesTested = 0;
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFullTest;
                    break;

                case MachineMode.FirstTest:
                    this.machineVolatileDataProvider.LoadUnitsToTest = loadUnits;
                    this.machineVolatileDataProvider.BayTestNumber = bayNumber;
                    this.machineVolatileDataProvider.CyclesTested = 0;
                    this.machineVolatileDataProvider.Mode = MachineMode.SwitchingToFirstTest;
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

        #endregion
    }
}
