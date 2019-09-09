using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IElevatorWeightCheckMessageData : IMessageData
    {
        #region Properties

        decimal? Weight { get; set; }

        #endregion
    }
}
