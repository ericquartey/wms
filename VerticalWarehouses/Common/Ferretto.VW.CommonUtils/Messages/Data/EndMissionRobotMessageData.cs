using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class EndMissionRobotMessageData : IEndMissionRobotMessageData
    {
        #region Constructors

        public EndMissionRobotMessageData()
        {
        }

        public EndMissionRobotMessageData(
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
