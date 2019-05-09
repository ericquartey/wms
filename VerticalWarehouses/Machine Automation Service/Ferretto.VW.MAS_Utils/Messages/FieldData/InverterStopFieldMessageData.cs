using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterStopFieldMessageData : IInverterStopFieldMessageData
    {
        #region Constructors

        public InverterStopFieldMessageData(InverterIndex inverterToStop, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToStop = inverterToStop;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToStop { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
