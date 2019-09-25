using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord;
using Ferretto.VW.MAS.Utils.Enumerations;

// ReSharper disable InconsistentNaming
// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public abstract class InverterStatusBase : IInverterStatusBase
    {
        #region Fields

        protected IControlWord controlWord = new ControlWordBase();

        protected IStatusWord statusWord = new StatusWordBase();

        private ushort operatingMode;

        private byte systemIndex;

        #endregion

        #region Constructors

        protected InverterStatusBase(InverterIndex systemIndex)
        {
            this.systemIndex = (byte)systemIndex;
        }

        #endregion

        #region Properties

        public IControlWord CommonControlWord => this.controlWord;

        public IStatusWord CommonStatusWord => this.statusWord;

        public bool[] Inputs { get; protected set; }

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

                    case (ushort)InverterOperationMode.TableTravel:
                        this.controlWord = new TableTravelControlWord(this.controlWord);
                        this.statusWord = new TableTravelStatusWord(this.statusWord);
                        break;
                }
                this.operatingMode = value;
            }
        }

        public byte SystemIndex { get; }

        public InverterType Type { get; protected set; }

        #endregion

        #region Methods

        public abstract bool UpdateInputsStates(bool[] newInputStates);

        #endregion
    }
}
