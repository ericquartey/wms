using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;


namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterSetTimerFieldMessageData : FieldMessageData, IInverterSetTimerFieldMessageData
    {
        #region Constructors

        public InverterSetTimerFieldMessageData(
            InverterTimer inverterTimer,
            bool enable,
            int updateInterval,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.InverterTimer = inverterTimer;
            this.Enable = enable;
            this.UpdateInterval = updateInterval;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        public InverterIndex InverterIndex { get; set; }

        public InverterTimer InverterTimer { get; }

        public int UpdateInterval { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Timer:{this.InverterTimer} Enable:{this.Enable} Interval:{this.UpdateInterval}";
        }

        #endregion
    }
}
