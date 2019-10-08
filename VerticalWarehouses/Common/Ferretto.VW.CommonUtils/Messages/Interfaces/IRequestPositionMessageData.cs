using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IRequestPositionMessageData : IMessageData
    {
        #region Properties

        Axis CurrentAxis { get; }

        #endregion
    }
}
