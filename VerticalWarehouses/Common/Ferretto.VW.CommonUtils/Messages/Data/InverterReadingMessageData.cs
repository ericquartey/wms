using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterReadingMessageData : IInverterReadingMessageData
    {
        #region Constructors

        public InverterReadingMessageData()
        {
        }

        public InverterReadingMessageData(IEnumerable<InverterParametersData> inverterParametersData, CommandAction commandAction = CommandAction.Start, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterParametersData = inverterParametersData;
            this.CommandAction = commandAction;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public CommandAction CommandAction { get; }

        public IEnumerable<InverterParametersData> InverterParametersData { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
