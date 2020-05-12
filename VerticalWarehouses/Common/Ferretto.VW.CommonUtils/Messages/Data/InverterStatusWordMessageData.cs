using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;


namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterStatusWordMessageData : IInverterStatusWordMessageData
    {
        #region Constructors

        public InverterStatusWordMessageData()
        {
        }

        public InverterStatusWordMessageData(byte inverterIndex, ushort value, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterIndex = inverterIndex;
            this.Value = value;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public byte InverterIndex { get; set; }

        public ushort Value { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
