using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface ISwitchAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToSwitch { get; }

        #endregion
    }
}
