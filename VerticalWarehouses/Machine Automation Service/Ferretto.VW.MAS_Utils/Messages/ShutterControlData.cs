using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
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
