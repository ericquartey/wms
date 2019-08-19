using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMachineStatusActiveMessageData : IMessageData
    {
        #region Properties

        MessageActor MessageActor { get; set; }

        string MessageType { get; set; }

        #endregion
    }
}
