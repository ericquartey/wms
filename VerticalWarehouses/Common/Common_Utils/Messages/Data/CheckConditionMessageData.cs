using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
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
