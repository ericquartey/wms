using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IStopAxisMessageData : IMessageData
    {
        #region Properties

        Axis AxisToStop { get; }

        #endregion
    }
}
