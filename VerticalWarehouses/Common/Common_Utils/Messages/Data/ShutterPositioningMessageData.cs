using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ShutterPositioningMessageData : IShutterPositioningMessageData
    {
        // TEMP
        //public ShutterPositioningMessageData(ShutterPositioningMovementMessageDataDTO dto)
        //{
        //    this.ShutterPositionMovement = dto.ShutterPositionMovement;
        //    this.BayNumber = dto.BayNumber;
        //    this.ShutterType = dto.ShutterType;
        //}

        #region Constructors

        public ShutterPositioningMessageData(ShutterMovementDirection shutterMovementDirection, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPositionMovement = shutterMovementDirection;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(ShutterPosition shutterPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPositionMovement = ShutterMovementDirection.None;
            this.ShutterPosition = ShutterPosition;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int BayNumber { get; }

        public ShutterPosition ShutterPosition { get; }

        public ShutterMovementDirection ShutterPositionMovement { get; }

        public int ShutterType { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
