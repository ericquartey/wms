using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IStopAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToStop { get; }

        #endregion
    }
}
