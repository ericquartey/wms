using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ISwitchAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToSwitch { get; }

        #endregion
    }
}
