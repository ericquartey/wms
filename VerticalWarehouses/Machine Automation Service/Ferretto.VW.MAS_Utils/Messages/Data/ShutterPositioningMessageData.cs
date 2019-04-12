using Ferretto.VW.MAS_Utils.DTOs;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class ShutterPositioningMessageData : IShutterPositioningMessageData
    {
        #region Constructors

        public ShutterPositioningMessageData(int shutterPositionMovement, MessageVerbosity verbosity = MessageVerbosity.Debug, FieldNotificationMessage fieldNotificationMessage = null)
        {
            this.ShutterPositionMovement = shutterPositionMovement;
            this.FieldMessage = fieldNotificationMessage;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(ShutterPosition shutterPosition, MessageVerbosity verbosity = MessageVerbosity.Debug, FieldNotificationMessage fieldNotificationMessage = null)
        {
            this.ShutterPosition = shutterPosition;
            this.FieldMessage = fieldNotificationMessage;
            this.Verbosity = verbosity;
        }

        public ShutterPositioningMessageData(ShutterPositioningMovementMessageDataDTO dto)
        {
            this.ShutterPositionMovement = dto.ShutterPositionMovement;
            this.BayNumber = dto.BayNumber;
            this.ShutterType = dto.ShutterType;
        }

        public ShutterPositioningMessageData(ShutterPosition shutterPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int BayNumber { get; }

        public FieldNotificationMessage FieldMessage { get; }

        public ShutterPosition ShutterPosition { get; }

        public int ShutterPositionMovement { get; }

        public int ShutterType { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
