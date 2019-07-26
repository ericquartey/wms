using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus
{
    public class MachineSensorsStatus : IMachineSensorsStatus
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 16;

        //private readonly IOSensorsStatus ioSensorsStatus;

        private readonly bool[] sensorStatus;

        #endregion

        #region Constructors

        public MachineSensorsStatus()
        {
            //INFO hp: the sensorStatus array contains the Remote IO sensor status between index 0 and 47
            // followed by the Inverter sensor between index 48 and 111
            this.sensorStatus = new bool[3 * REMOTEIO_INPUTS + INVERTER_INPUTS];
        }

        #endregion

        #region Properties

        public bool[] DisplayedInputs => this.sensorStatus;

        public bool IsDrawerCompletelyOnCradleBay1 => this.sensorStatus[(int)IOMachineSensors.LuPresentiInMachineSideBay1] && this.DisplayedInputs[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

        public bool IsDrawerPartiallyOnCradleBay1 => this.sensorStatus[(int)IOMachineSensors.LuPresentiInMachineSideBay1] != this.DisplayedInputs[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

        public bool IsDrawerInBay1Up => this.DisplayedInputs[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerPartiallyOnCradle => this.DisplayedInputs[(int)IOMachineSensors.LuPresentiInMachineSide] != this.DisplayedInputs[(int)IOMachineSensors.LuPresentInOperatorSide];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool IsMachineInEmergencyStateBay1 => !this.sensorStatus[(int)IOMachineSensors.NormalStateBay1];

        public bool IsZeroOnCradle => this.sensorStatus[(int)IOMachineSensors.ZeroPawl];

        public bool IsZeroOnElevator => this.sensorStatus[(int)IOMachineSensors.ZeroVertical];

        #endregion

        #region Methods

        //INFO Inputs from the inverter
        public bool UpdateInputs(byte ioIndex, bool[] newSensorStatus, FieldMessageActor messageActor)
        {
            var requiredUpdate = false;
            var updateDone = false;

            if (newSensorStatus == null)
            {
                return updateDone;
            }

            if (messageActor == FieldMessageActor.IoDriver)
            {
                if (ioIndex < 0 || ioIndex > 2)
                {
                    return false;
                }

                for (var index = 0; index < REMOTEIO_INPUTS; index++)
                {
                    if (this.sensorStatus[(ioIndex * 16) + index] != newSensorStatus[index])
                    {
                        requiredUpdate = true;
                        break;
                    }
                }

                if (requiredUpdate)
                {
                    Array.Copy(newSensorStatus, 0, this.sensorStatus, 0, REMOTEIO_INPUTS);
                    updateDone = true;
                }
            }

            requiredUpdate = false;

            if (messageActor == FieldMessageActor.InverterDriver)
            {
                for (var index = 0; index < INVERTER_INPUTS; index++)
                {
                    if (this.sensorStatus[index + REMOTEIO_INPUTS] != newSensorStatus[index])
                    {
                        requiredUpdate = true;
                        break;
                    }
                }

                if (requiredUpdate)
                {
                    Array.Copy(newSensorStatus, 0, this.sensorStatus, REMOTEIO_INPUTS, INVERTER_INPUTS);
                    updateDone = true;
                }
            }

            return updateDone;
        }

        #endregion
    }
}
