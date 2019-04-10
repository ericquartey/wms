namespace Ferretto.VW.MAS_Utils.DTOs
{
    public class MovementMessageDataDTO
    {
        #region Constructors

        public MovementMessageDataDTO(int axis, int movementType, uint speedPercentage, decimal? displacement = null, int? shutterDisplacement = null)
        {
            this.Displacement = displacement;
            this.ShutterDisplacement = shutterDisplacement;
            this.Axis = axis;
            this.MovementType = movementType;
            this.SpeedPercentage = speedPercentage;
        }

        #endregion

        #region Properties

        public int Axis { get; set; }

        public decimal? Displacement { get; set; }

        public int MovementType { get; set; }

        public int? ShutterDisplacement { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
