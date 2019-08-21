using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterStatusWordFieldMessageData : IInverterStatusWordFieldMessageData
    {
        #region Constructors

        public InverterStatusWordFieldMessageData(ushort value, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Value = value;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public ushort Value { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
