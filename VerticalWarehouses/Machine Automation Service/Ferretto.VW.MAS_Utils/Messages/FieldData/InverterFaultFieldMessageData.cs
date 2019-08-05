using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterFaultFieldMessageData : IInverterFaultFieldMessageData
    {
        #region Constructors

        public InverterFaultFieldMessageData(InverterIndex inverterToReset, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToReset = inverterToReset;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToReset { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
