using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CheckConditionMessageData : ICheckConditionMessageData
    {
        #region Constructors

        public CheckConditionMessageData()
        {
        }

        public CheckConditionMessageData(ConditionToCheckType conditionToCheck, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ConditionToCheck = conditionToCheck;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public ConditionToCheckType ConditionToCheck { get; set; }

        public bool Result { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
