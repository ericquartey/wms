using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMovementMessageData : IMessageData
    {
        #region Properties

        Axis Axis { get; set; }

        double? Displacement { get; set; }

        MovementType MovementType { get; set; }

        uint SpeedPercentage { get; set; }

        #endregion
    }
}
