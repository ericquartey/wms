using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class MovementMessageData : IMovementMessageData
    {
        #region Constructors

        public MovementMessageData(double? displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            this.Displacement = displacement;
            this.SpeedPercentage = speedPercentage;
            this.Axis = axis;
            this.MovementType = movementType;
        }

        #endregion

        #region Properties

        public Axis Axis { get; set; }

        public double? Displacement { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Axis:{this.Axis.ToString()} Displacement:{this.Displacement} MovementType:{this.MovementType.ToString()} SpeedPercentage:{this.SpeedPercentage}";
        }

        #endregion
    }
}
