using System;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    public class MachineResourcesProvider : IMachineResourcesProvider, ISensorsProvider
    {
        #region Fields

        private const int INVERTER_INPUTS = 8;

        private const int REMOTEIO_INPUTS = 16;

        private readonly ILogger<MachineResourcesProvider> logger;

        private readonly IMachineProvider machineProvider;

        /// <summary>
        /// It contains the Remote IO sensor status between index 0 and 47
        /// followed by the Inverter sensor between index 48 and 111.
        /// </summary>
        private readonly bool[] sensorStatus = new bool[3 * REMOTEIO_INPUTS + INVERTER_INPUTS * 8];

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool enableNotificatons;

        #endregion

        #region Constructors

        public MachineResourcesProvider(
            IMachineProvider machineProvider,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MachineResourcesProvider> logger)
        {
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.logger = logger;
        }

        #endregion

        #region Events

        public event EventHandler<StatusUpdateEventArgs> FaultStateChanged;

        public event EventHandler<StatusUpdateEventArgs> RunningStateChanged;

        #endregion

        #region Properties

        public bool[] DisplayedInputs => this.sensorStatus;

        public bool IsAntiIntrusionBarrierBay1 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay1];

        public bool IsAntiIntrusionBarrierBay2 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay2];

        public bool IsAntiIntrusionBarrierBay3 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay3];

        public bool IsDrawerCompletelyOffCradle => !this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && !this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerCompletelyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerInBay1Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay1];

        public bool IsDrawerInBay1Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerInBay2Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay2];

        public bool IsDrawerInBay2Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay2];

        public bool IsDrawerInBay3Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay3];

        public bool IsDrawerInBay3Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay3];

        public bool IsDrawerPartiallyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] != this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsInverterInFault => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool IsMachineInEmergencyState => !this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMachineInFaultState => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        public bool IsMachineInRunningState => this.machineProvider.IsMachineRunning;

        public bool IsMachineSecurityRunning => this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMicroCarterLeftSide => this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSide];

        public bool IsMicroCarterRightSide => this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSide];

        public bool IsMushroomEmergencyButtonBay1 => this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay1];

        public bool IsMushroomEmergencyButtonBay2 => this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay2];

        public bool IsMushroomEmergencyButtonBay3 => this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay3];

        public bool IsProfileCalibratedBay1 => this.sensorStatus[(int)IOMachineSensors.ProfileCalibrationBay1];

        public bool IsProfileCalibratedBay2 => this.sensorStatus[(int)IOMachineSensors.ProfileCalibrationBay2];

        public bool IsProfileCalibratedBay3 => this.sensorStatus[(int)IOMachineSensors.ProfileCalibrationBay3];

        public bool IsSensorZeroOnBay1 => this.sensorStatus[(int)IOMachineSensors.ACUBay1S3IND];

        public bool IsSensorZeroOnBay2 => this.sensorStatus[(int)IOMachineSensors.ACUBay2S3IND];

        public bool IsSensorZeroOnBay3 => this.sensorStatus[(int)IOMachineSensors.ACUBay3S3IND];

        public bool IsSensorZeroOnCradle => (this.machineProvider.IsOneTonMachine() ? this.sensorStatus[(int)IOMachineSensors.ZeroPawlSensorOneTon] : this.sensorStatus[(int)IOMachineSensors.ZeroPawlSensor]);

        public bool IsSensorZeroOnElevator => this.sensorStatus[(int)IOMachineSensors.ZeroVerticalSensor];

        #endregion

        #region Methods

        public void EnableNotification(bool enable)
        {
            this.enableNotificatons = enable;
        }

        public bool[] GetAll()
        {
            return (bool[])this.sensorStatus.Clone();
        }

        public ShutterPosition GetShutterPosition(InverterIndex inverterIndex)
        {
            var inverterStatus = new AglInverterStatus(inverterIndex, this.serviceScopeFactory);

            var sensorStart = (int)(IOMachineSensors.PowerOnOff + (byte)inverterStatus.SystemIndex * inverterStatus.Inputs.Length);

            Array.Copy(this.sensorStatus, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);

            return inverterStatus.CurrentShutterPosition;
        }

        public bool IsBayLightOn(BayNumber bayNumber)
        {
            if (this.machineProvider.IsBayLightOn.TryGetValue(bayNumber, out var isLight))
            {
                return isLight;
            }
            return false;
        }

        public bool IsDrawerInBayBottom(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1Bottom;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2Bottom;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3Bottom;
            }
        }

        public bool IsDrawerInBayTop(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1Top;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2Top;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3Top;
            }
        }

        public bool IsLoadingUnitInLocation(LoadingUnitLocation location)
        {
            bool returnValue;

            // TODO Update with missing information
            switch (location)
            {
                case LoadingUnitLocation.InternalBay1Down:
                    returnValue = this.IsDrawerInBay1Bottom;
                    break;

                case LoadingUnitLocation.InternalBay1Up:
                    returnValue = this.IsDrawerInBay1Top;
                    break;

                case LoadingUnitLocation.InternalBay2Down:
                    returnValue = this.IsDrawerInBay2Bottom;
                    break;

                case LoadingUnitLocation.InternalBay2Up:
                    returnValue = this.IsDrawerInBay2Top;
                    break;

                case LoadingUnitLocation.InternalBay3Down:
                    returnValue = this.IsDrawerInBay3Bottom;
                    break;

                case LoadingUnitLocation.InternalBay3Up:
                    returnValue = this.IsDrawerInBay3Top;
                    break;

                case LoadingUnitLocation.Elevator:
                    returnValue = this.IsDrawerCompletelyOnCradle;
                    break;

                // If location can't be idientifies simply become a "no operation" function
                default:
                    returnValue = true;
                    break;
            }

            return returnValue;
        }

        public bool IsMachineSecureForRun()
        {
            var returnValue = true;

            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay1])
            {
                returnValue = false;
            }

            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay2])
            {
                returnValue = false;
            }

            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay3])
            {
                returnValue = false;
            }

            if (this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSide])
            {
                returnValue = false;
            }

            if (this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSide])
            {
                returnValue = false;
            }

            return returnValue;
        }

        public bool IsProfileCalibratedBay(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsProfileCalibratedBay1;

                case BayNumber.BayTwo:
                    return this.IsProfileCalibratedBay2;

                case BayNumber.BayThree:
                    return this.IsProfileCalibratedBay3;
            }
        }

        public bool IsSensorZeroOnBay(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsSensorZeroOnBay1;

                case BayNumber.BayTwo:
                    return this.IsSensorZeroOnBay2;

                case BayNumber.BayThree:
                    return this.IsSensorZeroOnBay3;
            }
        }

        public void OnFaultStateChanged(StatusUpdateEventArgs e)
        {
            if (e.NewState != this.IsMachineInFaultState)
            {
                this.sensorStatus[(int)IOMachineSensors.InverterInFault1] = e.NewState;
                var handler = this.FaultStateChanged;
                handler?.Invoke(this, e);
            }
        }

        //INFO Inputs from the inverter
        public bool UpdateInputs(byte ioIndex, bool[] newSensorStatus, FieldMessageActor messageActor)
        {
            if (newSensorStatus is null)
            {
                return false;
            }

            try
            {
                var requiredUpdate = false;
                var updateDone = false;

                switch (messageActor)
                {
                    case FieldMessageActor.IoDriver:
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
                                if (ioIndex == 0 && this.enableNotificatons)
                                {
                                    if (this.sensorStatus[(int)IOMachineSensors.RunningState] !=
                                        newSensorStatus[(int)IOMachineSensors.RunningState])
                                    {
                                        //During Fault Handling running status will be set off. This prevents double firing the power off procedure
                                        if (!this.IsInverterInFault)
                                        {
                                            var args = new StatusUpdateEventArgs();
                                            args.NewState = newSensorStatus[(int)IOMachineSensors.RunningState];
                                            this.OnRunningStateChanged(args);
                                        }
                                    }

                                    if (this.sensorStatus[(int)IOMachineSensors.InverterInFault1] !=
                                        newSensorStatus[(int)IOMachineSensors.InverterInFault1])
                                    {
                                        var args = new StatusUpdateEventArgs();
                                        args.NewState = newSensorStatus[(int)IOMachineSensors.InverterInFault1];
                                        this.OnFaultStateChanged(args);
                                    }
                                }

                                Array.Copy(newSensorStatus, 0, this.sensorStatus, (ioIndex * REMOTEIO_INPUTS), REMOTEIO_INPUTS);
                                updateDone = true;
                            }

                            break;
                        }

                    case FieldMessageActor.InverterDriver:
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

                            break;
                        }
                }

                return updateDone;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while updating inputs");
                return false;
            }
        }

        protected virtual void OnRunningStateChanged(StatusUpdateEventArgs e)
        {
            var handler = this.RunningStateChanged;
            handler?.Invoke(this, e);
        }

        #endregion
    }
}
