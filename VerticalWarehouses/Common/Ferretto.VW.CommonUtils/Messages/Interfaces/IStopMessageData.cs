using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IStopMessageData : IMessageData
    {


        #region Properties

        StopRequestReason StopReason { get; }

        #endregion
    }
}
