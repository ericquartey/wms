using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterOperationTimeoutFieldMessageData : FieldMessageData, IInverterOperationTimeoutFieldMessageData
    {
        #region Constructors

        public InverterOperationTimeoutFieldMessageData(
            ushort controlWord,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.ControlWord = controlWord;
        }

        #endregion

        #region Properties

        public ushort ControlWord { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"ControlWord:{this.ControlWord}";
        }

        #endregion
    }
}
