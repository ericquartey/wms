namespace Ferretto.VW.Common_Utils.DTOs
{
    public class BeltBurnishingMessageDataDTO
    {
        #region Constructors

        public BeltBurnishingMessageDataDTO(int cyclesQuantity)
        {
            this.CyclesQuantity = cyclesQuantity;
        }

        #endregion

        #region Properties

        public int CyclesQuantity { get; set; }

        #endregion
    }
}
