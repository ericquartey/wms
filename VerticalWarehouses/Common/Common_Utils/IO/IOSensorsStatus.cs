using System;
using System.IO;
using Ferretto.VW.CommonUtils.Enumerations;

namespace Ferretto.VW.CommonUtils.IO
{
    public class IOSensorsStatus
    {
        #region Fields

        private const int TOTALSENSOR_INPUTS = 32;

        private readonly bool[] inputs;

        #endregion

        #region Constructors

        public IOSensorsStatus()
        {
            this.inputs = new bool[TOTALSENSOR_INPUTS];
        }

        #endregion

        #region Properties

        public bool AntiIntrusionShutterBay1 => this.inputs?[(int)IOMachineSensors.AntiIntrusionShutterBay1] ?? false;

        public bool AntiIntrusionShutterBay2 => this.inputs?[(int)IOMachineSensors.AntiIntrusionShutterBay2] ?? false;

        public bool AntiIntrusionShutterBay3 => this.inputs?[(int)IOMachineSensors.AntiIntrusionShutterBay3] ?? false;

        public bool CradleMotorSelected => this.inputs?[(int)IOMachineSensors.CradleMotorSelected] ?? false;

        public bool ElevatorMotorSelected => this.inputs?[(int)IOMachineSensors.ElevatorMotorSelected] ?? false;

        public bool EmergencyEndRun => this.inputs?[(int)IOMachineSensors.EmergencyEndRun] ?? false;

        public bool HeightControlCheckBay1 => this.inputs?[(int)IOMachineSensors.HeightControlCheckBay1] ?? false;

        public bool HeightControlCheckBay2 => this.inputs?[(int)IOMachineSensors.HeightControlCheckBay2] ?? false;

        public bool HeightControlCheckBay3 => this.inputs?[(int)IOMachineSensors.HeightControlCheckBay3] ?? false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Code may need to be refactored")]
        public bool[] Inputs => this.inputs;

        public bool LuPresentiInMachineSide => this.inputs?[(int)IOMachineSensors.LuPresentiInMachineSide] ?? false;

        public bool LuPresentInBay1 => this.inputs?[(int)IOMachineSensors.LUPresentInBay1] ?? false;

        public bool LuPresentInBay2 => this.inputs?[(int)IOMachineSensors.LUPresentInBay2] ?? false;

        public bool LuPresentInBay3 => this.inputs?[(int)IOMachineSensors.LUPresentInBay3] ?? false;

        public bool LuPresentInOperatorSide => this.inputs?[(int)IOMachineSensors.LuPresentInOperatorSide] ?? false;

        public bool MicroCarterLeftSideBay1 => this.inputs?[(int)IOMachineSensors.MicroCarterLeftSideBay1] ?? false;

        public bool MicroCarterLeftSideBay2 => this.inputs?[(int)IOMachineSensors.MicroCarterLeftSideBay2] ?? false;

        public bool MicroCarterLeftSideBay3 => this.inputs?[(int)IOMachineSensors.MicroCarterLeftSideBay3] ?? false;

        public bool MicroCarterRightSideBay1 => this.inputs?[(int)IOMachineSensors.MicroCarterRightSideBay1] ?? false;

        public bool MicroCarterRightSideBay2 => this.inputs?[(int)IOMachineSensors.MicroCarterRightSideBay2] ?? false;

        public bool MicroCarterRightSideBay3 => this.inputs?[(int)IOMachineSensors.MicroCarterRightSideBay3] ?? false;

        public bool MushroomHeadButtonBay1 => this.inputs?[(int)IOMachineSensors.MushroomHeadButtonBay1] ?? false;

        public bool MushroomHeadButtonBay2 => this.inputs?[(int)IOMachineSensors.MushroomHeadButtonBay2] ?? false;

        public bool MushroomHeadButtonBay3 => this.inputs?[(int)IOMachineSensors.MushroomHeadButtonBay3] ?? false;

        public bool SecurityFunctionActive => this.inputs?[(int)IOMachineSensors.NormalState] ?? false;

        public bool ShutterSensorABay1 => this.inputs?[(int)IOMachineSensors.ShutterSensorABay1] ?? false;

        public bool ShutterSensorABay2 => this.inputs?[(int)IOMachineSensors.ShutterSensorABay2] ?? false;

        public bool ShutterSensorABay3 => this.inputs?[(int)IOMachineSensors.ShutterSensorABay3] ?? false;

        public bool ShutterSensorBBay1 => this.inputs?[(int)IOMachineSensors.ShutterSensorBBay1] ?? false;

        public bool ShutterSensorBBay2 => this.inputs?[(int)IOMachineSensors.ShutterSensorBBay2] ?? false;

        public bool ShutterSensorBBay3 => this.inputs?[(int)IOMachineSensors.ShutterSensorBBay3] ?? false;

        public bool ZeroPawl => this.inputs?[(int)IOMachineSensors.ZeroPawl] ?? false;

        public bool ZeroVertical => this.inputs?[(int)IOMachineSensors.ZeroVertical] ?? false;

        #endregion

        #region Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Minor Code Smell",
            "S1227:break statements should not be used except for switch cases",
            Justification = "Code to be reviewed.")]
        public bool UpdateInputStates(bool[] newInputStates)
        {
            if (newInputStates == null)
            {
                return false;
            }

            var updateRequired = false;
            for (var index = 0; index < newInputStates.Length; index++)
            {
                if (index > TOTALSENSOR_INPUTS)
                {
                    continue;
                }

                if (this.inputs[index] != newInputStates[index])
                {
                    updateRequired = true;
                    break;
                }
            }
            try
            {
                if (updateRequired)
                {
                    Array.Copy(newInputStates, 0, this.inputs, 0, newInputStates.Length);
                }
            }
            catch (Exception ex)
            {
                throw new IOException($"Exception {ex.Message} while updating Inputs status");
            }

            return updateRequired;
        }

        #endregion
    }
}
