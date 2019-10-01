using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.DTOs
{
    public class CarouselMovementParameters
    {
        #region Properties

        public double Displacement { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
