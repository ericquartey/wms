using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ShutterPositionFieldMessageData : IShutterPositionFieldMessageData
    {
        #region Constructors

        public ShutterPositionFieldMessageData(ShutterPosition shutterPosition, byte systemIndex, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Verbosity = verbosity;

            this.ShutterPosition = shutterPosition;

            this.SystemIndex = systemIndex;
        }

        #endregion

        #region Properties

        public ShutterPosition ShutterPosition { get; }

        public byte SystemIndex { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
