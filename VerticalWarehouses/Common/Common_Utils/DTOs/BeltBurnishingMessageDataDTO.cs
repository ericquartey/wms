namespace Ferretto.VW.CommonUtils.DTOs
{
    public class BeltBurnishingMessageDataDto
    {
        #region Constructors

        public BeltBurnishingMessageDataDto(int cyclesQuantity)
        {
            this.CyclesQuantity = cyclesQuantity;
        }

        #endregion

        #region Properties

        public int CyclesQuantity { get; set; }

        #endregion
    }
}
