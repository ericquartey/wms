using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ICheckConditionMessageData : IMessageData
    {
        #region Properties

        ConditionToCheckType ConditionToCheck { get; set; }

        bool Result { get; set; }

        #endregion
    }
}
