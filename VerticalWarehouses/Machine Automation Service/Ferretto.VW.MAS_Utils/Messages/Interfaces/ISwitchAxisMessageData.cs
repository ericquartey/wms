using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface ISwitchAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToSwitch { get; }

        #endregion
    }
}
