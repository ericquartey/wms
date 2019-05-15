using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.ControlWord;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.StatusWord;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public abstract class InverterStatusBase : IInverterStatusBase
    {
        #region Fields

        protected IControlWord controlWord;

        protected IStatusWord statusWord;

        private ushort operatingMode;

        private byte systemIndex;

        #endregion

        #region Constructors

        protected InverterStatusBase()
        {
            this.controlWord = new ControlWordBase();
            this.statusWord = new StatusWordBase();
        }

        #endregion

        #region Properties

        public IControlWord CommonControlWord => this.controlWord;

        public IStatusWord CommonStatusWord => this.statusWord;

        public ushort OperatingMode
        {
            get => this.operatingMode;
            set
            {
                switch (value)
                {
                    case (ushort)InverterOperationMode.Homing:
                        this.controlWord = new HomingControlWord(this.controlWord);
                        this.statusWord = new HomingStatusWord(this.statusWord);
                        break;

                    case (ushort)InverterOperationMode.Position:
                        this.controlWord = new PositionControlWord(this.controlWord);
                        this.statusWord = new PositionStatusWord(this.statusWord);
                        break;
                }
                this.operatingMode = value;
            }
        }

        public byte SystemIndex
        {
            get => this.systemIndex;
            protected set => this.systemIndex = value;
        }

        #endregion
    }
}
