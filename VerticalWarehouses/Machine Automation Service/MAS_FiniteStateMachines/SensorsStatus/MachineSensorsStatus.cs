using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.IO;
using Ferretto.VW.MAS.IODriver.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus
{
    public class MachineSensorsStatus
    {
        #region Fields

        private const int TOTAL_INPUTS = 64;

        private readonly IOSensorsStatus ioSensorsStatus;

        readonly private bool[] rawInvertersInputs;

        readonly private bool[] rawRemoteIOsInputs;

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

        public bool DrawerIsCompletelyOnCradle => this.DisplayedInputs[(int)IOMachineSensors.LuPresentiInMachineSide] && this.DisplayedInputs[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool DrawerIsPartiallyOnCradle => this.DisplayedInputs[(int)IOMachineSensors.LuPresentiInMachineSide] != this.DisplayedInputs[(int)IOMachineSensors.LuPresentInOperatorSide];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool MachineIsInEmergencyState => !this.DisplayedInputs[(int)IOMachineSensors.NormalState];

        public bool[] RawInvertersInputs => this.rawInvertersInputs;

        public bool[] RawRemoteIOsInputs => this.rawRemoteIOsInputs;

        public bool SensorInZeroOnCradle => this.DisplayedInputs[(int)IOMachineSensors.ZeroPawl];

        public bool SensorInZeroOnElevator => this.DisplayedInputs[(int)IOMachineSensors.ZeroVertical];

        #endregion

        #region Methods

        public bool UpdateInputs(byte ioIndex, bool[] newRawInputs, FieldMessageActor messageActor)
        {
            var requiredUpdateIoInverters = false;

            if (newRawInputs == null)
            {
                return requiredUpdateIoInverters;
            }

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
                if (ioIndex < 0)
                {
                    return requiredUpdateRemoteIos;
                }
                if (ioIndex > 2)
                {
                    return requiredUpdateRemoteIos;
                }

                for (var index = 0; index < newRawInputs.Length; index++)
                {
                    if (this.rawRemoteIOsInputs[(ioIndex * 16) + index] != newRawInputs[index])
                    {
                        this.rawRemoteIOsInputs[(ioIndex * 16) + index] = newRawInputs[index];
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

            // TODO Rename the index of the rawInvertersInputs array.

            // TODO Change the method to update the sensors status to be displayed in the UI
            var newInputs = new bool[N_TOT_CHANNELS];

            // Bay1
            newInputs[(int)IOMachineSensors.NormalState] = this.rawRemoteIOsInputs[(int)IoPorts.NormalState];
            newInputs[(int)IOMachineSensors.MushroomHeadButtonBay1] = this.rawRemoteIOsInputs[(int)IoPorts.MushroomEmergency];
            newInputs[(int)IOMachineSensors.MicroCarterLeftSideBay1] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterLeftSideBay];
            newInputs[(int)IOMachineSensors.MicroCarterRightSideBay1] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterRightSideBay];
            newInputs[(int)IOMachineSensors.AntiIntrusionShutterBay1] = this.rawRemoteIOsInputs[(int)IoPorts.AntiIntrusionShutterBay];
            newInputs[(int)IOMachineSensors.LUPresentInBay1] = this.rawRemoteIOsInputs[(int)IoPorts.LoadingUnitExistenceInBay];
            newInputs[(int)IOMachineSensors.HeightControlCheckBay1] = this.rawRemoteIOsInputs[(int)IoPorts.HeightControlCheckBay];
            newInputs[(int)IOMachineSensors.ElevatorMotorSelected] = this.rawRemoteIOsInputs[(int)IoPorts.ElevatorMotorFeedback];
            newInputs[(int)IOMachineSensors.CradleMotorSelected] = this.rawRemoteIOsInputs[(int)IoPorts.CradleMotorFeedback];

            newInputs[(int)IOMachineSensors.EmergencyEndRun] = this.rawInvertersInputs[0];

            newInputs[(int)IOMachineSensors.ZeroVertical] = this.rawInvertersInputs[2];

            newInputs[(int)IOMachineSensors.ElevatorMotorSelected] = this.rawRemoteIOsInputs[(int)IoPorts.ElevatorMotorFeedback];
            newInputs[(int)IOMachineSensors.CradleMotorSelected] = this.rawRemoteIOsInputs[(int)IoPorts.CradleMotorFeedback];

            newInputs[(int)IOMachineSensors.LuPresentiInMachineSide] = this.rawRemoteIOsInputs[(int)IoPorts.DrawerInMachineSide];
            newInputs[(int)IOMachineSensors.LuPresentInOperatorSide] = this.rawRemoteIOsInputs[(int)IoPorts.DrawerInOperatorSide];

            // Bay2
            newInputs[(int)IOMachineSensors.MushroomHeadButtonBay2] = this.rawRemoteIOsInputs[(int)IoPorts.MushroomEmergency + 16];
            newInputs[(int)IOMachineSensors.MicroCarterLeftSideBay2] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterLeftSideBay + 16];
            newInputs[(int)IOMachineSensors.MicroCarterRightSideBay2] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterRightSideBay + 16];
            newInputs[(int)IOMachineSensors.AntiIntrusionShutterBay2] = this.rawRemoteIOsInputs[(int)IoPorts.AntiIntrusionShutterBay + 16];
            newInputs[(int)IOMachineSensors.LUPresentInBay2] = this.rawRemoteIOsInputs[(int)IoPorts.LoadingUnitExistenceInBay + 16];
            newInputs[(int)IOMachineSensors.HeightControlCheckBay2] = this.rawRemoteIOsInputs[(int)IoPorts.HeightControlCheckBay + 16];

            // Bay3
            newInputs[(int)IOMachineSensors.MushroomHeadButtonBay3] = this.rawRemoteIOsInputs[(int)IoPorts.MushroomEmergency + 32];
            newInputs[(int)IOMachineSensors.MicroCarterLeftSideBay3] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterLeftSideBay + 32];
            newInputs[(int)IOMachineSensors.MicroCarterRightSideBay3] = this.rawRemoteIOsInputs[(int)IoPorts.MicroCarterRightSideBay + 32];
            newInputs[(int)IOMachineSensors.AntiIntrusionShutterBay3] = this.rawRemoteIOsInputs[(int)IoPorts.AntiIntrusionShutterBay + 32];
            newInputs[(int)IOMachineSensors.LUPresentInBay3] = this.rawRemoteIOsInputs[(int)IoPorts.LoadingUnitExistenceInBay + 32];
            newInputs[(int)IOMachineSensors.HeightControlCheckBay3] = this.rawRemoteIOsInputs[(int)IoPorts.HeightControlCheckBay + 32];

            this.ioSensorsStatus?.UpdateInputStates(newInputs);
        }

        #endregion
    }
}
