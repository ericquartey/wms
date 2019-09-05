using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMachineStateActiveMessageData : IMessageData
    {
        #region Properties

        MessageActor MessageActor { get; set; }

        string CurrentState { get; set; }

        #endregion
    }
}
