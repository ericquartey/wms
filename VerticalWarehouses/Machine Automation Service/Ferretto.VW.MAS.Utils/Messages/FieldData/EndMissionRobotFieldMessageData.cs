using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class EndMissionRobotFieldMessageData : FieldMessageData, IEndMissionRobotFieldMessageData
    {
        #region Constructors

        public EndMissionRobotFieldMessageData(bool enable = false, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.Enable = enable;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        #endregion
    }
}
