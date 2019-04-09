using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages
{
    public class ShutterControlData : IShutterControlData
    {
        #region Constructors

        public ShutterControlData(int delay, int numberCycles)
        {
            this.Delay = delay;

            this.NumberCycles = numberCycles;
        }

        #endregion

        #region Properties

        public int Delay { get; set; }

        public int NumberCycles { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
