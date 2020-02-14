using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterFaultFieldMessageData : FieldMessageData, IInverterFaultFieldMessageData
    {
        #region Constructors

        public InverterFaultFieldMessageData(
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion
    }
}
