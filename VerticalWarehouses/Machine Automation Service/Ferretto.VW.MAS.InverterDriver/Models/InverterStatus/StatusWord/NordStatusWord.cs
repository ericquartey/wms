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

        public bool Active481_10 => (this.Value & 0x2000) > 0;

        public bool Active481_9 => (this.Value & 0x0400) > 0;

        public bool BusControlActive => (this.Value & 0x0200) > 0;

        public bool ParameterSet1 => (this.Value & 0x4000) > 0;

        public bool ParameterSet2 => (this.Value & 0x8000) > 0;

        public bool RotationLeft => (this.Value & 0x1000) > 0;

        public bool RotationRight => (this.Value & 0x0800) > 0;

        public bool SetpointReached => (this.Value & 0x0100) > 0;

        #endregion
    }
}
