using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.Interfaces
{
    public interface IStopAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToStop { get; }

        #endregion
    }
}
