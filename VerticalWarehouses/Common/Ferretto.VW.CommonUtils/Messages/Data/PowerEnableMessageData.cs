using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class PowerEnableMessageData : IPowerEnableMessageData
    {
        #region Constructors

        public PowerEnableMessageData()
        {
        }

        public PowerEnableMessageData(bool enable, CommandAction commandAction = CommandAction.Start, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.CommandAction = commandAction;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public bool Enable { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
