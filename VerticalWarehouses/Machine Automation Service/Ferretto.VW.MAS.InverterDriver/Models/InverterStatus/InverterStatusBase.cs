using System.Text;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.ControlWord;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.StatusWord;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public abstract class InverterStatusBase : IInverterStatusBase
    {
        #region Fields

        protected IControlWord controlWord = new ControlWordBase();

        protected IStatusWord statusWord = new StatusWordBase();

        private ushort operatingMode;

        #endregion

        #region Constructors

        protected InverterStatusBase(InverterIndex systemIndex)
        {
            this.SystemIndex = systemIndex;
        }

        #endregion

        #region Properties

        public IControlWord CommonControlWord => this.controlWord;

        public IStatusWord CommonStatusWord => this.statusWord;

        public bool[] Inputs { get; protected set; }

        public bool IsStarted =>
            this.CommonStatusWord.IsReadyToSwitchOn &
            this.CommonStatusWord.IsSwitchedOn &
            this.CommonStatusWord.IsVoltageEnabled &
            this.CommonStatusWord.IsQuickStopTrue;

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

                    case (ushort)InverterOperationMode.Nord:
                        this.controlWord = new NordControlWord(this.controlWord);
                        this.statusWord = new NordStatusWord(this.statusWord);
                        break;
                }
                this.operatingMode = value;
            }
        }

        public InverterIndex SystemIndex { get; }

        #endregion

        #region Methods

        public string InputsToString()
        {
            var sb = new StringBuilder();
            foreach (var b in this.Inputs)
            {
                sb.AppendFormat("{0} ", b ? 1 : 0);
            }

            return $"[{sb}]";
        }

        public abstract bool UpdateInputsStates(bool[] newInputStates);

        #endregion
    }
}
