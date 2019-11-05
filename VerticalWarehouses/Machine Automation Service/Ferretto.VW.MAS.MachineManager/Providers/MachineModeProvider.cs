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

        private readonly IMachineModeDataProvider machineModeDataProvider;

        #endregion

        #region Constructors

        public MachineModeProvider(
            IMachineModeDataProvider machineModeDataProvider,
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

            if (machineMode is MachineMode.Automatic)
            {
                this.machineModeDataProvider.Mode = MachineMode.SwitchingToAutomatic;
            }
            else if (machineMode is MachineMode.Manual)
            {
                this.machineModeDataProvider.Mode = MachineMode.SwitchingToManual;
            }
            else
            {
                throw new ArgumentException(nameof(machineMode));
            }

            this.SendCommandToMissionManager(
                new MachineModeMessageData(machineMode),
                $"Request mode change to '{machineMode}'",
                MessageActor.MissionManager,
                MessageType.MachineMode,
                BayNumber.All);
        }

        #endregion
    }
}
