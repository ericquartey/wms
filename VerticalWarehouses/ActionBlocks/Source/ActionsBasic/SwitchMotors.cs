using System.Collections.Generic;
using System.Threading;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.RemoteIODriver;

namespace Ferretto.VW.ActionBlocks
{
    public class SwitchMotors : ISwitchMotors
    {
        #region Fields

        private const int ENCODER_CRADLE = 2;  // 1

        private const int ENCODER_ELEVATOR = 1;  // 0

        private const int N_DIGITAL_OUTPUT_LINES = 5;

        private byte dataSetIndex = 0x05;

        private InverterDriver.InverterDriver inverterDriver; // class instance

        private IRemoteIO remoteIO; // interface

        private byte systemIndex = 0x00;

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialize.
        /// </summary>
        public void Initialize()
        {
            // TODO Add your implementation code here
        }

        /// <summary>
        /// Switch from horizontal to vertical engine control.
        /// Use the same inverter to handle the the elevator and the cradle.
        /// </summary>
        public void SwitchHorizToVert()
        {
            var DigitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add(false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(250);

            DigitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add((i == ENCODER_ELEVATOR) ? true : false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(250);

            var value = (ushort)0x0000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);
        }

        /// <summary>
        /// Switch from vertical to horizontal engine control.
        /// Use the same inverter to handle the the elevator and the cradle.
        /// </summary>
        public void SwitchVertToHoriz()
        {
            var DigitalOutput = new List<bool>();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add(false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(250);

            DigitalOutput.Clear();
            for (var i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput.Add((i == ENCODER_CRADLE) ? true : false);
            }
            this.remoteIO.Outputs = DigitalOutput;
            Thread.Sleep(250);

            var value = (ushort)0x8000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);
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
