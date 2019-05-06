using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class ShutterControlMessageData : IShutterControlMessageData
    {
        #region Constructors

        public ShutterControlMessageData(int delay, int numberCycles)
        {
            this.Delay = delay;

            this.NumberCycles = numberCycles;
        }

        #endregion

        #region Properties

        public int CurrentShutterPosition { get; set; }

        public int Delay { get; set; }

        public int NumberCycles { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
