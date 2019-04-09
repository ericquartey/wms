namespace Ferretto.VW.Common_Utils.DTOs
{
    public class MovementMessageDataDTO
    {
        #region Constructors

        public MovementMessageDataDTO(decimal displacement, int axis, int movementType, uint speedPercentage)
        {
            this.Displacement = displacement;
            this.Axis = axis;
            this.MovementType = movementType;
            this.SpeedPercentage = speedPercentage;
        }

        #endregion

        #region Properties

        public int Axis { get; set; }

        public decimal Displacement { get; set; }

        public int MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
