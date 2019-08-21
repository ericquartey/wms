using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ShutterTestStatusChangedMessageData : IShutterTestStatusChangedMessageData
    {
        #region Constructors

        public ShutterTestStatusChangedMessageData()
        {
        }

        public ShutterTestStatusChangedMessageData(int bayNumber, int delay, int numberCycles, int speedRate)
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

        #region Methods

        public override string ToString()
        {
            return $"BayNumber:{this.BayNumber} CurrentShutterPosition:{this.CurrentShutterPosition} Delay:{this.Delay} ExecutedCycles:{this.ExecutedCycles} NumberCycles:{this.NumberCycles} ShutterType:{this.ShutterType.ToString()} SpeedRate:{this.SpeedRate}";
        }

        #endregion
    }
}
