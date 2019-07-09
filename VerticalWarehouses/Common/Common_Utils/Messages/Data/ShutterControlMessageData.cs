using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ShutterControlMessageData : IShutterControlMessageData
    {
        #region Constructors

        public ShutterControlMessageData()
        {
        }

        public ShutterControlMessageData(int bayNumber, int delay, int numberCycles, int speedRate)
        {
            this.Delay = delay;
            this.NumberCycles = numberCycles;
            this.BayNumber = bayNumber;
            this.SpeedRate = speedRate;
        }

        #endregion

        #region Properties

        public int BayNumber { get; set; }

        public int CurrentShutterPosition { get; set; }

        public int Delay { get; set; }

        public int ExecutedCycles { get; set; }

        public int NumberCycles { get; set; }

        public ShutterType ShutterType { get; set; }

        public int SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
