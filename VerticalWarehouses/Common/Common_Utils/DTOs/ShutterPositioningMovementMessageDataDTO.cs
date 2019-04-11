namespace Ferretto.VW.Common_Utils.DTOs
{
    public class ShutterPositioningMovementMessageDataDTO
    {
        #region Constructors

        public ShutterPositioningMovementMessageDataDTO(int bayNumber, int shutterPositionMovement)
        {
            this.ShutterPositionMovement = shutterPositionMovement;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public int BayNumber { get; set; }

        public int ShutterPositionMovement { get; set; }

        public int ShutterType { get; set; }

        #endregion
    }
}
