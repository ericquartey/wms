using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.RemoteIODriver.Source;

namespace Ferretto.VW.ActionBlocks
{
    public class SwitchMotors : ISwitchMotors
    {
        #region Fields

        private const int N_DIGITAL_OUTPUT_LINES = 5;
        private const int ENCODER_ELEVATOR = 0;
        private const int ENCODER_CRADLE = 1;
        private byte systemIndex = 0x00;
        private byte dataSetIndex = 0x05;
        private RemoteIO remoteIO;
        private InverterDriver.InverterDriver inverterDriver;

        #endregion Fields

        #region Properties

        public RemoteIO SetRemoteIOInterface
        {

            set => this.remoteIO = value;
        }

        public InverterDriver.InverterDriver SetInverterDriverInterface
        {
            set => this.inverterDriver = value;
        }

        #endregion Properties

        #region Method

        public void Initialize()
        {
           
        }

        public void SwitchHorizToVert()
        {
            var DigitalOutput = new bool[N_DIGITAL_OUTPUT_LINES];
            for (int i = 0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput[i] = false;
            }
            this.remoteIO.WriteData(DigitalOutput);
            DigitalOutput[ENCODER_ELEVATOR] = true;
            this.remoteIO.WriteData(DigitalOutput);
            var value = (ushort)0x0000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);

        }

        public void SwitchVertToHoriz()
        {
            var DigitalOutput = new bool[N_DIGITAL_OUTPUT_LINES];
            for(int i=0; i < N_DIGITAL_OUTPUT_LINES; i++)
            {
                DigitalOutput[i] = false;
            }
            this.remoteIO.WriteData(DigitalOutput);
            DigitalOutput[ENCODER_CRADLE] = true;
            this.remoteIO.WriteData(DigitalOutput);
            var value = (ushort)0x8000;
            this.inverterDriver.SettingRequest(ParameterID.CONTROL_WORD_PARAM, this.systemIndex, this.dataSetIndex, value);

        }

        public void Terminate()
        {

        }

        #endregion Method
    }
}
