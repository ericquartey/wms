using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.DTOs
{
    public class ShutterPositioningMovementMessageDataDto
    {
        #region Constructors

        public ShutterPositioningMovementMessageDataDto(ShutterMovementDirection shutterPositionMovement, int bayNumber)
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
