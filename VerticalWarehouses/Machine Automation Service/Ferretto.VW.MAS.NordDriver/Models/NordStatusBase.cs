using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;

namespace Ferretto.VW.MAS.NordDriver
{
    public class NordStatusBase : INordStatusBase
    {
        #region Fields

        protected IControlWord controlWord = new ControlWordBase();

        protected IStatusWord statusWord = new StatusWordBase();

        private ushort operatingMode;

        #endregion

        #region Constructors

        public NordStatusBase(InverterIndex systemIndex)
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
            //this.CommonStatusWord.IsSwitchedOn &
            //this.CommonStatusWord.IsVoltageEnabled &
            this.CommonStatusWord.IsQuickStopTrue;

        public ushort OperatingMode
        {
            get => this.operatingMode;
            set
            {
                //switch (value)
                //{
                //    case (ushort)InverterOperationMode.Homing:
                //        this.controlWord = new HomingControlWord(this.controlWord);
                //        this.statusWord = new HomingStatusWord(this.statusWord);
                //        break;

                //    case (ushort)InverterOperationMode.Position:
                //        this.controlWord = new PositionControlWord(this.controlWord);
                //        this.statusWord = new PositionStatusWord(this.statusWord);
                //        break;

                //    case (ushort)InverterOperationMode.ProfileVelocity:
                //        this.controlWord = new ProfileVelocityControlWord(this.controlWord);
                //        this.statusWord = new ProfileVelocityStatusWord(this.statusWord);
                //        break;

                //    case (ushort)InverterOperationMode.TableTravel:
                //        this.controlWord = new TableTravelControlWord(this.controlWord);
                //        this.statusWord = new TableTravelStatusWord(this.statusWord);
                //        break;
                //}
                this.operatingMode = value;
            }
        }

        public ushort SetPointFrequency { get; set; }

        public int SetPointPosition { get; set; }

        public ushort SetPointRampTime { get; set; }

        public InverterIndex SystemIndex { get; }

        #endregion

        #region Methods

        public bool UpdateInputsStates(bool[] newInputStates)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
