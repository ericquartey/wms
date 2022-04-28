using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord
{
    public class TableTravelStatusWord : StatusWordBase, ITableTravelStatusWord
    {
        #region Constructors

        public TableTravelStatusWord(ushort controlWordValue)
            : base(controlWordValue)
        {
        }

        public TableTravelStatusWord(IStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        public TableTravelStatusWord(ITableTravelStatusWord otherControlWord)
            : base(otherControlWord.Value)
        {
        }

        #endregion

        #region Properties

        public bool FollowingError => (this.Value & 0x2000) > 0;

        public bool InGear => (this.Value & 0x1000) > 0;

        public bool InternalLimitActive => (this.Value & 0x0800) > 0;

        public bool MotionBlockInProgress => (this.Value & 0x0100) > 0;

        public bool TargetReached => (this.Value & 0x0400) > 0;

        #endregion
    }
}
