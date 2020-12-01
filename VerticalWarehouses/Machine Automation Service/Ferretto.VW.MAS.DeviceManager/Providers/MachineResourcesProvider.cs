using System;
using System.Text;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    public class MachineResourcesProvider : IMachineResourcesProvider, ISensorsProvider
    {
        #region Fields

        private const int INVERTER_INPUTS = 8;

        private const int REMOTEIO_INPUTS = 16;

        private readonly ILogger<MachineResourcesProvider> logger;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

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
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<MachineResourcesProvider> logger)
        {
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
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

        public bool IsDeviceManagerBusy => this.machineVolatileDataProvider.IsDeviceManagerBusy;

        public bool IsDrawerCompletelyOffCradle => !this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && !this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerCompletelyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerInBay1Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay1];

        public bool IsDrawerInBay1ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerInBay1InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay1];

        public bool IsDrawerInBay1Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerInBay2Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay2];

        public bool IsDrawerInBay2ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay2];

        public bool IsDrawerInBay2InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay2];

        public bool IsDrawerInBay2Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay2];

        public bool IsDrawerInBay3Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay3];

        public bool IsDrawerInBay3ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay3];

        public bool IsDrawerInBay3InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay3];

        public bool IsDrawerInBay3Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay3];

        public bool IsDrawerPartiallyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] != this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsElevatorOverrun => this.sensorStatus[(int)IOMachineSensors.ElevatorOverrun];

        public bool IsInverterInFault => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool IsMachineInEmergencyState => !this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMachineInFaultState => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        public bool IsMachineInRunningState => this.machineVolatileDataProvider.IsMachineRunning;

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

        public bool IsSensorZeroOnCradle => (this.machineVolatileDataProvider.IsOneTonMachine.Value ? this.sensorStatus[(int)IOMachineSensors.ZeroPawlSensorOneTon] : this.sensorStatus[(int)IOMachineSensors.ZeroPawlSensor]);

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
            if (inverterIndex == InverterIndex.None)
            {
                return ShutterPosition.NotSpecified;
            }

            var inverterStatus = new AglInverterStatus(inverterIndex, this.serviceScopeFactory);

            var sensorStart = (int)(IOMachineSensors.PowerOnOff + (byte)inverterStatus.SystemIndex * inverterStatus.Inputs.Length);

            Array.Copy(this.sensorStatus, sensorStart, inverterStatus.Inputs, 0, inverterStatus.Inputs.Length);

            return inverterStatus.CurrentShutterPosition;
        }

        public bool IsBayLightOn(BayNumber bayNumber)
        {
            if (this.machineVolatileDataProvider.IsBayLightOn.TryGetValue(bayNumber, out var isLight))
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

        public bool IsDrawerInBayExternalPosition(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1ExternalPosition;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2ExternalPosition;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3ExternalPosition;
            }
        }

        public bool IsDrawerInBayInternalPosition(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1InternalPosition;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2InternalPosition;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3InternalPosition;
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
                    returnValue = this.IsDrawerCompletelyOnCradle && !this.IsSensorZeroOnCradle;
                    break;

                // If location can't be idientifies simply become a "no operation" function
                default:
                    returnValue = true;
                    break;
            }

            return returnValue;
        }

        public bool IsMachineSecureForRun(out string errorText)
        {
            var isMarchPossible = true;
            var reason = new StringBuilder();

            if (this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSide])
            {
                isMarchPossible = false;
                reason.Append("Micro Carter Active Bay1 Left; ");
            }
            if (this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSide])
            {
                isMarchPossible = false;
                reason.Append("Micro Carter Active Bay1 Right; ");
            }

            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                foreach (var bay in baysDataProvider.GetAll())
                {
                    switch (bay.Number)
                    {
                        case BayNumber.BayOne:
                            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay1])
                            {
                                isMarchPossible = false;
                                reason.Append("Emergency Active Bay1; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay1])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay1; ");
                            }

                            break;

                        case BayNumber.BayTwo:
                            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay2])
                            {
                                isMarchPossible = false;
                                reason.Append("Emergency Active Bay2; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay2])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay2; ");
                            }

                            break;

                        case BayNumber.BayThree:
                            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay3])
                            {
                                isMarchPossible = false;
                                reason.Append("Emergency Active Bay3; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay3])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay3; ");
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            errorText = reason.ToString();
            return isMarchPossible;
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
                                if (this.sensorStatus[(int)IOMachineSensors.ElevatorOverrun] != newSensorStatus[(int)InverterSensors.ANG_ElevatorOverrunSensor]
                                    && newSensorStatus[(int)InverterSensors.ANG_ElevatorOverrunSensor]
                                    )
                                {
                                    var errorCode = (newSensorStatus[(int)InverterSensors.ANG_ZeroElevatorSensor]) ? MachineErrorCode.ElevatorUnderrunDetected : MachineErrorCode.ElevatorOverrunDetected;
                                    using (var scope = this.serviceScopeFactory.CreateScope())
                                    {
                                        scope.ServiceProvider
                                            .GetRequiredService<IErrorsProvider>()
                                            .RecordNew(errorCode);
                                    }
                                    if (this.machineVolatileDataProvider.Mode == MachineMode.Manual ||
                                        this.machineVolatileDataProvider.Mode == MachineMode.Manual2 ||
                                        this.machineVolatileDataProvider.Mode == MachineMode.Manual3)
                                    {
                                        this.logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                    }
                                    else
                                    {
                                        this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(BayNumber.All);//to fix
                                        this.logger.LogInformation($"Machine status switched to {MachineMode.Manual}");
                                    }
                                }

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
