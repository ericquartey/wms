using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMachineStateActiveMessageData : IMessageData
    {
        #region Properties

        string CurrentState { get; set; }

        MessageActor MessageActor { get; set; }

        #endregion
    }
}
