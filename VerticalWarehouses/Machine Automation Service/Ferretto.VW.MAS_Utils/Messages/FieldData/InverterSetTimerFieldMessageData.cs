using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterSetTimerFieldMessageData : FieldMessageData, IInverterSetTimerFieldMessageData
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
