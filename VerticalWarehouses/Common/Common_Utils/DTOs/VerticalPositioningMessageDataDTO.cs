namespace Ferretto.VW.CommonUtils.DTOs
{
    public class VerticalPositioningMessageDataDto
    {
        #region Constructors

        public VerticalPositioningMessageDataDto(int cyclesQuantity)
        {
            this.CyclesQuantity = cyclesQuantity;
        }

        #endregion

        #region Properties

        public int CyclesQuantity { get; set; }

        #endregion
    }
}
