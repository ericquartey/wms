using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.DTOs
{
    public class CarouselMovementParameters
    {
        #region Properties

        public decimal Displacement { get; set; }

        public MovementType MovementType { get; set; }

        public uint SpeedPercentage { get; set; }

        #endregion
    }
}
