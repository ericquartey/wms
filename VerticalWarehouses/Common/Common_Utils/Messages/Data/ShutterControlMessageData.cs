using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
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

        public ShutterType ShutterType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int SpeedRate { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
