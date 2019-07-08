using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterOperationTimeoutFieldMessageData : IInverterOperationTimeoutFieldMessageData
    {
        #region Constructors

        public InverterOperationTimeoutFieldMessageData(ushort controlWord, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ControlWord = controlWord;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public ushort ControlWord { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
