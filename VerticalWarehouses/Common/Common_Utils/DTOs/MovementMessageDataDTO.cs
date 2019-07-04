using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.DTOs
{
    public class MovementMessageDataDto
    {
        #region Constructors

        public MovementMessageDataDto(Axis axis, MovementType movementType, uint speedPercentage, decimal displacement)
        {
            this.Displacement = displacement;
            this.Axis = axis;
            this.MovementType = movementType;
            this.SpeedPercentage = speedPercentage;
        }

        #endregion

        #region Properties

        public Axis Axis { get; set; }

        public decimal Displacement { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
