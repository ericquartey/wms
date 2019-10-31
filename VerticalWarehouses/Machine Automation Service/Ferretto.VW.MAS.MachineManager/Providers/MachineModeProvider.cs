using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Data;
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

        public CommonUtils.Messages.MachineMode GetCurrent()
        {
            return this.machineModeDataProvider.Mode;
        }

        public void RequestChange(CommonUtils.Messages.MachineMode machineMode)
        {
            this.SendCommandToMissionManager(
                new MachineModeMessageData(machineMode),
                $"Request mode change to '{machineMode}'",
                MessageActor.NotSpecified,
                MessageType.MachineMode,
                BayNumber.All);
        }

        #endregion
    }
}
