using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_FiniteStateMachines.SensorsStatus
{
    public class MachineSensorsStatus
    {
        #region Fields

        //TEMP Total maximum inputs: 9 + 9 + (16 + 9 + 9) * 3 = 120
        private const int TOTAL_INPUTS = 64;

        private readonly IOSensorsStatus ioSensorsStatus;

        private bool[] rawInvertersInputs;

        private bool[] rawRemoteIOsInputs;

        #endregion

        #region Constructors

        public MachineSensorsStatus()
        {
            this.rawRemoteIOsInputs = new bool[TOTAL_INPUTS]; //TEMP RemoteIO input source
            this.rawInvertersInputs = new bool[TOTAL_INPUTS]; //TEMP Inverter input source

            this.ioSensorsStatus = new IOSensorsStatus(); //TEMP IO sensors status being displayed
        }

        #endregion

        #region Properties

        public bool[] DisplayedInputs => this.ioSensorsStatus?.Inputs;

        public bool[] RawInvertersInputs => this.rawInvertersInputs;

        public bool[] RawRemoteIOsInputs => this.rawRemoteIOsInputs;

        #endregion

        #region Methods

        public bool UpdateInputs(bool[] newRawInputs, FieldMessageActor messageActor)
        {
            var requiredUpdateIoInverters = false;

            if (messageActor == FieldMessageActor.InverterDriver)
            {
                for (var index = 0; index < newRawInputs.Length; index++)
                {
                    if (this.rawInvertersInputs[index] != newRawInputs[index])
                    {
                        this.rawInvertersInputs[index] = newRawInputs[index];
                        requiredUpdateIoInverters = true;
                    }
                }
            }

            var requiredUpdateRemoteIos = false;

            if (messageActor == FieldMessageActor.IoDriver)
            {
                for (var index = 0; index < newRawInputs.Length; index++)
                {
                    if (this.rawRemoteIOsInputs[index] != newRawInputs[index])
                    {
                        this.rawRemoteIOsInputs[index] = newRawInputs[index];
                        requiredUpdateRemoteIos = true;
                    }
                }
            }

            if (requiredUpdateRemoteIos || requiredUpdateIoInverters)
            {
                this.updateIoSensorsStatus();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void updateIoSensorsStatus()
        {
            const int N_TOT_CHANNELS = 32;

            // TODO Change the method to update the sensors status to be displayed in the UI
            var newInputs = new bool[N_TOT_CHANNELS];

            newInputs[(int)IOMachineSensors.SecurityFunctionActive] = this.rawRemoteIOsInputs[0];
            newInputs[(int)IOMachineSensors.MushroomHeadButtonBay1] = this.rawRemoteIOsInputs[1];
            newInputs[(int)IOMachineSensors.MicroCarterLeftSideBay1] = this.rawRemoteIOsInputs[2];
            newInputs[(int)IOMachineSensors.MicroCarterRightSideBay1] = this.rawRemoteIOsInputs[3];
            newInputs[(int)IOMachineSensors.AntiIntrusionShutterBay1] = this.rawRemoteIOsInputs[4];
            newInputs[(int)IOMachineSensors.LUPresentInBay1] = this.rawRemoteIOsInputs[5];
            newInputs[(int)IOMachineSensors.ElevatorMotorSelected] = this.rawRemoteIOsInputs[8];
            newInputs[(int)IOMachineSensors.CradleMotorSelected] = this.rawRemoteIOsInputs[9];

            newInputs[(int)IOMachineSensors.ZeroVertical] = this.rawInvertersInputs[2];
            newInputs[(int)IOMachineSensors.LuPresentiInMachineSide] = this.rawInvertersInputs[7];
            newInputs[(int)IOMachineSensors.LuPresentInOperatorSide] = this.rawInvertersInputs[6];

            this.ioSensorsStatus?.UpdateInputStates(newInputs);
        }

        #endregion
    }
}
