using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal class MachineModeProvider : BaseProvider, IMachineModeProvider
    {
        #region Fields

        private readonly IMachineModeVolatileDataProvider machineModeDataProvider;

        #endregion

        #region Constructors

        public MachineModeProvider(
            IMachineModeVolatileDataProvider machineModeDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.machineModeDataProvider = machineModeDataProvider ?? throw new ArgumentNullException(nameof(machineModeDataProvider));
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
                    break;

                case MachineMode.Manual:
                    this.machineModeDataProvider.Mode = MachineMode.SwitchingToManual;
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
            if (this.machineModeDataProvider.Mode is MachineMode.SwitchingToAutomatic)
            {
                this.machineModeDataProvider.Mode = MachineMode.Automatic;
            }
            else if (this.machineModeDataProvider.Mode is MachineMode.SwitchingToManual)
            {
                this.machineModeDataProvider.Mode = MachineMode.Manual;
            }

            // HACK: end
        }

        #endregion
    }
}
