using Ferretto.VW.MAS_InverterDriver.Enumerations;
using Ferretto.VW.MAS_InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.ControlWord;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_InverterDriver.InverterStatus.StatusWord;
using Ferretto.VW.MAS_Utils.Enumerations;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_InverterDriver.InverterStatus
{
    public abstract class InverterStatusBase : IInverterStatusBase
    {
        #region Fields

        protected IControlWord controlWord;

        protected IStatusWord statusWord;

        private InverterType inverterType;

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

        public InverterType InverterType
        {
            get => this.inverterType;
            protected set => this.inverterType = value;
        }

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

                    case (ushort)InverterOperationMode.ProfileVelocity:
                        this.controlWord = new ProfileVelocityControlWord(this.controlWord);
                        this.statusWord = new ProfileVelocityStatusWord(this.statusWord);
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
