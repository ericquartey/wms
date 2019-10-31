using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public class MachineModeMessageData : IMessageData
    {
        #region Constructors

        public MachineModeMessageData(CommonUtils.Messages.MachineMode machineMode, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.MachineMode = machineMode;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommonUtils.Messages.MachineMode MachineMode { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
