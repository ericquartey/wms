using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResetHardwareMessageData : IMessageData
    {
        #region Properties

        ResetOperation Operation { get; set; }

        #endregion
    }
}
