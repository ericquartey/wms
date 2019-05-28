using Ferretto.VW.Common_Utils.Messages.Enumerations;

namespace Ferretto.VW.Common_Utils.DTOs
{
    public class ShutterPositioningMovementMessageDataDTO
    {
        #region Constructors

        public ShutterPositioningMovementMessageDataDTO(ShutterMovementDirection shutterPositionMovement, int bayNumber)
        {
            this.ShutterPositionMovement = shutterPositionMovement;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public int BayNumber { get; set; }

        public ShutterMovementDirection ShutterPositionMovement { get; set; }

        public ShutterType ShutterType { get; set; }

        #endregion
    }
}
