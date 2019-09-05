using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.SensorsStatus
{

    public class MachineSensorsStatus : IMachineSensorsStatus
    {

        #region Fields

        private const int INVERTER_INPUTS = 8;

        private const int REMOTEIO_INPUTS = 16;

        private readonly bool[] sensorStatus;

        private decimal axisXPosition;

        private decimal axisYPosition;

        #endregion

        #region Constructors

        public MachineSensorsStatus()
        {
            //INFO hp: the sensorStatus array contains the Remote IO sensor status between index 0 and 47
            // followed by the Inverter sensor between index 48 and 111
            this.sensorStatus = new bool[3 * REMOTEIO_INPUTS + INVERTER_INPUTS * 8];
        }

        #endregion



        #region Events

        public event EventHandler<StatusUpdateEventArgs> FaultStateChanged;

        public event EventHandler<StatusUpdateEventArgs> RunningStateChanged;

        #endregion



        #region Properties

        public decimal AxisXPosition { get => this.axisXPosition; set => this.axisXPosition = value; }

        public decimal AxisYPosition { get => this.axisYPosition; set => this.axisYPosition = value; }

        public bool[] DisplayedInputs => this.sensorStatus;

        public bool IsDrawerCompletelyOffCradle => !this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSideBay1] && !this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

        public bool IsDrawerCompletelyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSideBay1] && this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

        public bool IsDrawerInBay1Up => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerPartiallyOnCradleBay1 => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSideBay1] != this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSideBay1];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool IsMachineInEmergencyStateBay1 => !this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMachineInFaultState => this.sensorStatus[(int)IOMachineSensors.InverterFault];

        public bool IsMachineInRunningState => this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsSensorZeroOnCradle => this.sensorStatus[(int)IOMachineSensors.ZeroPawlSensor];

        public bool IsSensorZeroOnElevator => this.sensorStatus[(int)IOMachineSensors.ZeroVerticalSensor];

        #endregion



        #region Methods

        //INFO Inputs from the inverter
        public bool UpdateInputs(byte ioIndex, bool[] newSensorStatus, FieldMessageActor messageActor)
        {
            try
            {
                var requiredUpdate = false;
                var updateDone = false;

                if (newSensorStatus == null)
                {
                    return false;
                }

                if (messageActor == FieldMessageActor.IoDriver)
                {
                    if (ioIndex > 2)
                    {
                        return false;
                    }

                    for (var index = 0; index < REMOTEIO_INPUTS; index++)
                    {
                        if (this.sensorStatus[(ioIndex * REMOTEIO_INPUTS) + index] != newSensorStatus[index])
                        {
                            requiredUpdate = true;
                            break;
                        }
                    }

                    if (requiredUpdate)
                    {
                        if (ioIndex == 0)
                        {
                            if (this.sensorStatus[(int)IOMachineSensors.RunningState] !=
                                newSensorStatus[(int)IOMachineSensors.RunningState])
                            {
                                StatusUpdateEventArgs args = new StatusUpdateEventArgs();
                                args.NewState = newSensorStatus[(int)IOMachineSensors.RunningState];
                                this.OnRunningStateChanged(args);
                            }

                            if (this.sensorStatus[(int)IOMachineSensors.InverterFault] !=
                                newSensorStatus[(int)IOMachineSensors.InverterFault])
                            {
                                StatusUpdateEventArgs args = new StatusUpdateEventArgs();
                                args.NewState = newSensorStatus[(int)IOMachineSensors.InverterFault];
                                this.OnFaultStateChanged(args);
                            }
                        }

                        Array.Copy(newSensorStatus, 0, this.sensorStatus, (ioIndex * REMOTEIO_INPUTS), REMOTEIO_INPUTS);
                        updateDone = true;
                    }
                }

                requiredUpdate = false;

                if (messageActor == FieldMessageActor.InverterDriver)
                {
                    for (var index = 0; index < INVERTER_INPUTS; index++)
                    {
                        if (this.sensorStatus[index + 3 * REMOTEIO_INPUTS + (ioIndex * INVERTER_INPUTS)] != newSensorStatus[index])
                        {
                            requiredUpdate = true;
                            break;
                        }
                    }

                    if (requiredUpdate)
                    {
                        Array.Copy(newSensorStatus, 0, this.sensorStatus, 3 * REMOTEIO_INPUTS + (ioIndex * INVERTER_INPUTS), newSensorStatus.Length);
                        updateDone = true;
                    }
                }

                return updateDone;
            }
            catch (Exception exc)
            {
                Console.WriteLine($@"{exc}");
                return false;
            }
        }

        protected virtual void OnFaultStateChanged(StatusUpdateEventArgs e)
        {
            EventHandler<StatusUpdateEventArgs> handler = FaultStateChanged;
            handler?.Invoke(this, e);
        }

        protected virtual void OnRunningStateChanged(StatusUpdateEventArgs e)
        {
            EventHandler<StatusUpdateEventArgs> handler = RunningStateChanged;
            handler?.Invoke(this, e);
        }

        #endregion
    }
}
