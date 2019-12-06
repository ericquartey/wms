using System;
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

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        #endregion

        #region Constructors

        public MachineModeProvider(
            IMachineModeVolatileDataProvider machineModeDataProvider,
            ILogger<MachineModeProvider> logger,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.machineModeDataProvider = machineModeDataProvider ?? throw new ArgumentNullException(nameof(machineModeDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public MachineMode GetCurrent()
        {
            return this.machineModeDataProvider.Mode;
        }

        public void RequestChange(MachineMode machineMode)
        {
            if (machineMode == this.machineModeDataProvider.Mode)
            {
                return;
            }

            switch (machineMode)
            {
                case MachineMode.Automatic:
                    this.machineModeDataProvider.Mode = MachineMode.SwitchingToAutomatic;
                    this.logger.LogInformation($"Machine status switched to {this.machineModeDataProvider.Mode}");
                    break;

                case MachineMode.Manual:
                    this.machineModeDataProvider.Mode = MachineMode.SwitchingToManual;
                    this.logger.LogInformation($"Machine status switched to {this.machineModeDataProvider.Mode}");
                    break;

                default:
                    throw new ArgumentException($"The requested machine mode '{machineMode}' cannot be handled.", nameof(machineMode));
            }

            this.SendCommandToMachineManager(
                new MachineModeMessageData(machineMode),
                $"Request mode change to '{machineMode}'",
                MessageActor.MissionManager,
                MessageType.MachineMode,
                BayNumber.All);

            // HACK: this is a mocked implementation of the mode switch
            // HACK: begin
            if (this.machineModeDataProvider.Mode is MachineMode.SwitchingToManual)
            {
                this.machineModeDataProvider.Mode = MachineMode.Manual;
                this.logger.LogInformation($"Machine status switched to {this.machineModeDataProvider.Mode}");
            }

            // HACK: end
        }

        #endregion
    }
}
