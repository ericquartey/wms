using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ICheckConditionMessageData : IMessageData
    {
        #region Properties

        ConditionToCheckType ConditionToCheck { get; set; }

        bool Result { get; set; }

        #endregion
    }
}
