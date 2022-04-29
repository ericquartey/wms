using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class NordStatusWord : StatusWordBase, INordStatusWord
    {
        #region Constructors

        public NordStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public NordStatusWord(IStatusWord otherStatusWord)
            : base(otherStatusWord?.Value ?? throw new System.ArgumentNullException(nameof(otherStatusWord)))
        {
        }

        public NordStatusWord(INordStatusWord otherControlWord)
            : base(otherControlWord?.Value ?? throw new System.ArgumentNullException(nameof(otherControlWord)))
        {
        }

        #endregion

        #region Properties

        public bool IsOperationEnabledNord => (this.Value & 0x0002) > 0;

        public bool IsQuickStopTrueNord => (this.Value & 0x0004) > 0;

        public bool ParameterSet1 => (this.Value & 0x0020) > 0;

        public bool ParameterSet2 => (this.Value & 0x0040) > 0;

        public bool TargetReached => (this.Value & 0x0010) > 0;

        #endregion
    }
}
