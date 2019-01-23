using System.Collections.Generic;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.RemoteIODriver;

namespace Ferretto.VW.ActionBlocks
{
    // On [EndedEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void SwitchMotorsEndedEventHandler();
    public delegate void SwitchMotorsVHEndedEventHandler();
    public delegate void SwitchMotorsHVEndedEventHandler();

    // On [ErrorEventHandler] delegate for Calibrate Vertical Axis routine
    public delegate void SwitchMotorsErrorEventHandler();

    public class SwitchMotors : ISwitchMotors
    {
        #region Events

        // [Ended] event
        public event SwitchMotorsEndedEventHandler ThrowEndEvent;
        public event SwitchMotorsVHEndedEventHandler ThrowVHEndEvent;
        public event SwitchMotorsHVEndedEventHandler ThrowHVEndEvent;

        // [Error] event
        public event SwitchMotorsErrorEventHandler ThrowErrorEvent;

        #endregion Events

        #region Fields

        private const int ENCODER_CRADLE = 2;  // 1

        private const int ENCODER_ELEVATOR = 1;  // 0

        private const int N_DIGITAL_OUTPUT_LINES = 5;

        private byte dataSetIndex = 0x05;

        private InverterDriver.InverterDriver inverterDriver; // class instance

        private IRemoteIO remoteIO; // interface

        private byte systemIndex = 0x00;

        private bool currentMotor; // false for the Horizontal motor and true for the Vertical motor

        // The delay time for the Sleep Function
        private const int DELAY_TIME = 250;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Set the inverter driver interface.
        /// </summary>
        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        /// <summary>
        /// Set the remoteIO interface.
        /// </summary>
        public IRemoteIO SetRemoteIOInterface
        {
            set => this.remoteIO = value;
        }

        /// <summary>
        /// Set the remoteIO interface.
        /// </summary>
        public bool SetCurrentMotor
        {
            set => this.currentMotor = value;
            get => currentMotor;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Initialize()
        {
            // TODO Add your implementation code here
        }

        public void callSwitchHorizToVert()
        {
            Thread switchHorizToVert;

            switchHorizToVert = new Thread(this.SwitchHorizToVert);
            switchHorizToVert.Start();


        }

        /// <summary>
        /// Switch from horizontal to vertical engine control.
        /// Use the same inverter to handle the the elevator and the cradle.
        /// </summary>
        private void SwitchHorizToVert()
        {
            var DigitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add(false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(DELAY_TIME);

            DigitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add((i == ENCODER_ELEVATOR) ? true : false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(DELAY_TIME);

            var value = (ushort)0x0000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);

            // The calibrate horizontal axis routine is ended
            ThrowEndEvent?.Invoke();
            // bThrowHVEndEvent?.Invoke();
        }

        public void callSwitchVertToHoriz()
        {
            Thread switchVertToHoriz;

            switchVertToHoriz = new Thread(this.SwitchVertToHoriz);
            switchVertToHoriz.Start();
        }

        /// <summary>
        /// Switch from vertical to horizontal engine control.
        /// Use the same inverter to handle the the elevator and the cradle.
        /// </summary>
        private void SwitchVertToHoriz()
        {
            var DigitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add(false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(DELAY_TIME);

            DigitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add((i == ENCODER_CRADLE) ? true : false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(DELAY_TIME);

            var value = (ushort)0x8000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);

            // The calibrate horizontal axis routine is ended
            ThrowEndEvent?.Invoke();
            // ThrowVHEndEvent?.Invoke();
        }

        /// <summary>
        /// Terminate.
        /// </summary>
        public void Terminate()
        {
            // TODO Add your implementation code here
        }

        #endregion Methods
    }
}
