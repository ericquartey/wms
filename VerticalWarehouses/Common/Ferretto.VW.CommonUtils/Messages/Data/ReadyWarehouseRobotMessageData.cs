using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ReadyWarehouseRobotMessageData : IReadyWarehouseRobotMessageData
    {
        #region Constructors

        public ReadyWarehouseRobotMessageData()
        {
        }

        public ReadyWarehouseRobotMessageData(
            bool enable,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
