using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositionFieldMessageData : IShutterPositionFieldMessageData
    {
        #region Constructors

        public ShutterPositionFieldMessageData(ShutterPosition shutterPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;

            this.ShutterPosition = shutterPosition;
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
