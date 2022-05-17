using System;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.InverterDriver.InverterStatus
{
    public class NordInverterStatus : InverterStatusBase, INordInverterStatus
    {
        #region Fields

        private const int TOTAL_SENSOR_INPUTS = 8;

        private ushort analogIn;

        private ushort current;

        private int currentPosition;

        #endregion

        #region Constructors

        public NordInverterStatus(InverterIndex systemIndex)
            : base(systemIndex)
        {
            this.Inputs = new bool[TOTAL_SENSOR_INPUTS];
            this.OperatingMode = (ushort)InverterOperationMode.Nord;
        }

        #endregion

        #region Properties

        public ushort AnalogIn => this.analogIn;

        public ushort Current => this.current;

        public int CurrentPosition => this.currentPosition;

        public bool IsStarted =>
            this.CommonStatusWord.IsReadyToSwitchOn &
            //this.CommonStatusWord.IsSwitchedOn &
            //this.CommonStatusWord.IsVoltageEnabled &
            this.CommonStatusWord.IsQuickStopTrue;

        public INordControlWord NordControlWord
        {
            get
            {
                if (this.controlWord is INordControlWord word)
                {
                    return word;
                }

                throw new InvalidOperationException($"Current Control Word Type {this.controlWord.GetType().Name} is not compatible with Nord Mode");
            }
        }

        public INordStatusWord NordStatusWord
        {
            get
            {
                if (this.statusWord is INordStatusWord word)
                {
                    return word;
                }

                throw new InvalidOperationException($"Current Status Word Type {this.statusWord.GetType().Name} is not compatible with Nord Mode");
            }
        }

        public ushort SetPointFrequency { get; set; }

        public ushort SetPointRampTime { get; set; }

        #endregion

        #region Methods

        public bool UpdateAnalogIn(ushort analogIn)
        {
            var updateRequired = false;

            if (this.analogIn != analogIn)
            {
                this.analogIn = analogIn;
                updateRequired = true;
            }

            return updateRequired;
        }

        public bool UpdateCurrent(ushort current)
        {
            var updateRequired = false;

            if (this.current != current)
            {
                this.current = current;
                updateRequired = true;
            }

            return updateRequired;
        }

        public override bool UpdateInputsStates(bool[] newInputStates)
        {
            if (newInputStates is null)
            {
                return false;
            }

            var updateRequired = false;
            for (var index = 0; index < newInputStates.Length; index++)
            {
                if (index > TOTAL_SENSOR_INPUTS)
                {
                    break;
                }

                if (this.Inputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }

            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.Inputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new InverterDriverException("Error while updating inverter inputs.", ex);
            }

            return updateRequired;
        }

        public bool UpdateInverterCurrentPosition(int position)
        {
            var updateRequired = false;

            if (this.currentPosition != position)
            {
                this.currentPosition = position;
                updateRequired = true;
            }

            return updateRequired;
        }

        #endregion
    }
}
