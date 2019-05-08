namespace Ferretto.VW.Common_Utils.DTOs
{
    public class VerticalPositioningMessageDataDTO
    {
        #region Constructors

        public VerticalPositioningMessageDataDTO(int cyclesQuantity)
        {
            this.CyclesQuantity = cyclesQuantity;
        }

        #endregion

        #region Properties

        public int CyclesQuantity { get; set; }

        #endregion
    }
}
