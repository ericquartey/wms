using System;
using System.Text;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.DeviceManager.SensorsStatus;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.Providers
{
    public class MachineResourcesProvider : IMachineResourcesProvider, ISensorsProvider
    {
        #region Fields

        private const int INVERTER_INPUTS = 8;

        private const int REMOTEIO_INPUTS = 16;

        private const int REMOTEIO_OUTPUTS = 8;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly ILogger<MachineResourcesProvider> logger;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        /// <summary>
        /// It contains the Remote IO sensor status between index 0 and 47
        /// followed by the Inverter sensor between index 48 and 111.
        /// </summary>
        private readonly bool[] sensorStatus = new bool[3 * REMOTEIO_INPUTS + INVERTER_INPUTS * 8];

        private readonly bool[] outFault = new bool[3 * REMOTEIO_OUTPUTS * 8];
        private readonly int[] outCurrent = new int[3 * REMOTEIO_OUTPUTS * 8];

        private readonly IServiceScopeFactory serviceScopeFactory;

        private bool enableNotificatons;

        #endregion

        #region Constructors

        public MachineResourcesProvider(
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IServiceScopeFactory serviceScopeFactory,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            ILogger<MachineResourcesProvider> logger)
        {
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.logger = logger;
        }

        #endregion

        #region Events

        public event EventHandler<StatusUpdateEventArgs> FaultStateChanged;

        public event EventHandler<StatusUpdateEventArgs> RunningStateChanged;

        public event EventHandler<StatusUpdateEventArgs> SecurityStateChanged;

        #endregion

        #region Properties

        public bool[] DisplayedInputs => this.sensorStatus;

        public bool FireAlarm => this.IsFireAlarmActive() ? this.sensorStatus[(int)IOMachineSensors.RobotOptionBay1] : false;

        public bool IsAntiIntrusionBarrier2Bay1 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrier2Bay1];

        public bool IsAntiIntrusionBarrier2Bay2 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrier2Bay2];

        public bool IsAntiIntrusionBarrier2Bay3 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrier2Bay3];

        public bool IsAntiIntrusionBarrierBay1 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay1];

        public bool IsAntiIntrusionBarrierBay2 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay2];

        public bool IsAntiIntrusionBarrierBay3 => this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay3];

        public bool IsDeviceManagerBusy => this.machineVolatileDataProvider.IsDeviceManagerBusy;

        public bool IsDrawerCompletelyOffCradle => !this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && !this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerCompletelyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] && this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsDrawerInBay1Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay1];

        public bool IsDrawerInBay1ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerInBay1InternalBottom => this.sensorStatus[(int)IOMachineSensors.RobotOptionBay1];

        public bool IsDrawerInBay1InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay1];

        public bool IsDrawerInBay1InternalTop => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay1];

        public bool IsDrawerInBay1Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay1];

        public bool IsDrawerInBay2Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay2];

        public bool IsDrawerInBay2ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay2];

        public bool IsDrawerInBay2InternalBottom => this.sensorStatus[(int)IOMachineSensors.RobotOptionBay2];

        public bool IsDrawerInBay2InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay2];

        public bool IsDrawerInBay2InternalTop => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay2];

        public bool IsDrawerInBay2Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay2];

        public bool IsDrawerInBay3Bottom => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay3];

        public bool IsDrawerInBay3ExternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay3];

        public bool IsDrawerInBay3InternalBottom => this.sensorStatus[(int)IOMachineSensors.RobotOptionBay3];

        public bool IsDrawerInBay3InternalPosition => this.sensorStatus[(int)IOMachineSensors.LUPresentMiddleBottomBay3];

        public bool IsDrawerInBay3InternalTop => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay3];

        public bool IsDrawerInBay3Top => this.sensorStatus[(int)IOMachineSensors.LUPresentInBay3];

        public bool IsDrawerPartiallyOnCradle => this.sensorStatus[(int)IOMachineSensors.LuPresentInMachineSide] != this.sensorStatus[(int)IOMachineSensors.LuPresentInOperatorSide];

        public bool IsElevatorOverrun => this.sensorStatus[(int)IOMachineSensors.ElevatorOverrun] && !this.sensorStatus[(int)IOMachineSensors.ZeroVerticalSensor];

        public bool IsElevatorUnderrun => this.sensorStatus[(int)IOMachineSensors.ElevatorOverrun] && this.sensorStatus[(int)IOMachineSensors.ZeroVerticalSensor];

        public bool IsInverterInFault => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        //TEMP SecurityFunctionActive means the machine is in operative mode (vs the emergency mode)
        public bool IsMachineInEmergencyState => !this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMachineInFaultState => this.sensorStatus[(int)IOMachineSensors.InverterInFault1];

        public bool IsMachineInRunningState => this.machineVolatileDataProvider.IsMachineRunning;

        public bool IsMachineSecurityRunning => this.sensorStatus[(int)IOMachineSensors.RunningState];

        public bool IsMicroCarterLeftSideBay1 => this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay1];

        public bool IsMicroCarterLeftSideBay2 => this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay2];

        public bool IsMicroCarterLeftSideBay3 => this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay3];

        public bool IsMicroCarterRightSideBay1 => this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay1];

        public bool IsMicroCarterRightSideBay2 => this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay2];

        public bool IsMicroCarterRightSideBay3 => this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay3];

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

        public bool IsSensorZeroTopOnBay1 => this.sensorStatus[(int)IOMachineSensors.ACUBay1S6IND];

        public bool IsSensorZeroTopOnBay2 => this.sensorStatus[(int)IOMachineSensors.ACUBay2S6IND];

        public bool IsSensorZeroTopOnBay3 => this.sensorStatus[(int)IOMachineSensors.ACUBay3S6IND];

        public bool PreFireAlarm => this.IsFireAlarmActive() ? this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay1] : false;

        public bool TeleOkBay1 => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay1];

        public bool TeleOkBay2 => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay2];

        public bool TeleOkBay3 => this.sensorStatus[(int)IOMachineSensors.TrolleyOptionBay3];

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

        public bool[] GetOutFault()
        {
            return (bool[])this.outFault.Clone();
        }

        public int[] GetOutCurrent()
        {
            return (int[])this.outCurrent.Clone();
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

        public bool IsDrawerInBayBottom(BayNumber bayNumber, bool isExternalDouble)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1Bottom || this.IsDrawerInBay1InternalBottom;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2Bottom || this.IsDrawerInBay2InternalBottom;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3Bottom || this.IsDrawerInBay3InternalBottom;
            }
        }

        public bool IsDrawerInBayExternalPosition(BayNumber bayNumber, bool isExternalDoubleBay)
        {
            if (isExternalDoubleBay)
            {
                switch (bayNumber)
                {
                    default:
                    case BayNumber.BayOne:
                        return this.IsDrawerInBay1Top ||
                            this.IsDrawerInBay1Bottom;

                    case BayNumber.BayTwo:
                        return this.IsDrawerInBay2Top ||
                            this.IsDrawerInBay2Bottom;

                    case BayNumber.BayThree:
                        return this.IsDrawerInBay3Top ||
                            this.IsDrawerInBay3Bottom;
                }
            }
            else
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
        }

        public bool IsDrawerInBayInternalBottom(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1InternalBottom;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2InternalBottom;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3InternalBottom;
            }
        }

        public bool IsDrawerInBayInternalPosition(BayNumber bayNumber, bool isDouble)
        {
            if (isDouble)
            {
                switch (bayNumber)
                {
                    default:
                    case BayNumber.BayOne:
                        return this.IsDrawerInBay1InternalTop || this.IsDrawerInBay1InternalBottom;

                    case BayNumber.BayTwo:
                        return this.IsDrawerInBay2InternalTop || this.IsDrawerInBay2InternalBottom;

                    case BayNumber.BayThree:
                        return this.IsDrawerInBay3InternalTop || this.IsDrawerInBay3InternalBottom;
                }
            }
            else
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
        }

        public bool IsDrawerInBayInternalTop(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1InternalTop;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2InternalTop;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3InternalTop;
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

        public bool IsDrawerInBayTop(BayNumber bayNumber, bool isExternalDouble)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsDrawerInBay1Top || this.IsDrawerInBay1InternalTop;

                case BayNumber.BayTwo:
                    return this.IsDrawerInBay2Top || this.IsDrawerInBay2InternalTop;

                case BayNumber.BayThree:
                    return this.IsDrawerInBay3Top || this.IsDrawerInBay3InternalTop;
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

        public bool IsMachineSecureForRun(out string errorText, out MachineErrorCode errorCode, out BayNumber bayNumber)
        {
            var isMarchPossible = true;
            var reason = new StringBuilder();
            errorCode = MachineErrorCode.NoError;
            bayNumber = BayNumber.None;

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
                                errorCode = MachineErrorCode.SecurityButtonWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay1])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay1; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay1])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay1 Left; ");
                                errorCode = MachineErrorCode.SecurityLeftSensorWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay1])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay1 Right; ");
                                errorCode = MachineErrorCode.SecurityRightSensorWasTriggered;
                                bayNumber = bay.Number;
                            }

                            break;

                        case BayNumber.BayTwo:
                            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay2])
                            {
                                isMarchPossible = false;
                                reason.Append("Emergency Active Bay2; ");
                                errorCode = MachineErrorCode.SecurityButtonWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay2])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay2; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay2])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay2 Left; ");
                                errorCode = MachineErrorCode.SecurityLeftSensorWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay2])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay2 Right; ");
                                errorCode = MachineErrorCode.SecurityRightSensorWasTriggered;
                                bayNumber = bay.Number;
                            }

                            break;

                        case BayNumber.BayThree:
                            if (this.sensorStatus[(int)IOMachineSensors.MushroomEmergencyButtonBay3])
                            {
                                isMarchPossible = false;
                                reason.Append("Emergency Active Bay3; ");
                                errorCode = MachineErrorCode.SecurityButtonWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.AntiIntrusionBarrierBay3])
                            {
                                //isMarchPossible = false;
                                reason.Append("Anti Intrusion Barrier Active Bay3; ");
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterLeftSideBay3])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay3 Left; ");
                                errorCode = MachineErrorCode.SecurityLeftSensorWasTriggered;
                                bayNumber = bay.Number;
                            }
                            else if (this.sensorStatus[(int)IOMachineSensors.MicroCarterRightSideBay3])
                            {
                                isMarchPossible = false;
                                reason.Append("Micro Carter Active Bay3 Right; ");
                                errorCode = MachineErrorCode.SecurityRightSensorWasTriggered;
                                bayNumber = bay.Number;
                            }

                            break;

                        default:
                            break;
                    }
                }

                if (this.FireAlarm)
                {
                    isMarchPossible = false;
                    reason.Append("FireAlarm Active; ");
                    errorCode = MachineErrorCode.FireAlarm;
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

        public bool IsSensorZeroTopOnBay(BayNumber bayNumber)
        {
            switch (bayNumber)
            {
                default:
                case BayNumber.BayOne:
                    return this.IsSensorZeroTopOnBay1;

                case BayNumber.BayTwo:
                    return this.IsSensorZeroTopOnBay2;

                case BayNumber.BayThree:
                    return this.IsSensorZeroTopOnBay3;
            }
        }

        public void OnFaultStateChanged(StatusUpdateEventArgs e)
        {
            //if (e.NewState != this.IsMachineInFaultState)
            {
                this.sensorStatus[(int)IOMachineSensors.InverterInFault1] = e.NewState;
                var handler = this.FaultStateChanged;
                handler?.Invoke(this, e);
            }
        }

        public bool UpdateDiagOutFault(byte ioIndex, bool[] newOutFault)
        {
            if (newOutFault == null)
            {
                return false;
            }
            if (ioIndex > 2)
            {
                return false;
            }
            var requiredUpdate = false;
            for (var index = 0; index < REMOTEIO_OUTPUTS && !requiredUpdate; index++)
            {
                if (this.outFault[(ioIndex * REMOTEIO_OUTPUTS) + index] != newOutFault[index])
                {
                    requiredUpdate = true;
                }
            }
            if(requiredUpdate)
            {
                Array.Copy(newOutFault, 0, this.outFault, (ioIndex * REMOTEIO_OUTPUTS), REMOTEIO_OUTPUTS);
            }
            return requiredUpdate;
        }

        public bool UpdateDiagOutCurrent(byte ioIndex, int[] newOutCurrent)
        {
            if (newOutCurrent == null)
            {
                return false;
            }
            if (ioIndex > 2)
            {
                return false;
            }
            var requiredUpdate = false;
            for (var index = 0; index < REMOTEIO_OUTPUTS && !requiredUpdate; index++)
            {
                if (this.outCurrent[(ioIndex * REMOTEIO_OUTPUTS) + index] != newOutCurrent[index])
                {
                    requiredUpdate = true;
                }
            }
            if (requiredUpdate)
            {
                Array.Copy(newOutCurrent, 0, this.outCurrent, (ioIndex * REMOTEIO_OUTPUTS), REMOTEIO_OUTPUTS);
            }
            return requiredUpdate;
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

                            var ioSecurityChange = false;
                            var ioRunningStateChange = false;
                            var ioInverterFaultChange = false;
                            for (var index = 0; index < REMOTEIO_INPUTS; index++)
                            {
                                if (this.sensorStatus[(ioIndex * REMOTEIO_INPUTS) + index] != newSensorStatus[index])
                                {
                                    requiredUpdate = true;
                                    if (index == (int)IOMachineSensors.RunningState)
                                    {
                                        ioRunningStateChange = true;
                                    }
                                    if (index == (int)IOMachineSensors.InverterInFault1
                                        && newSensorStatus[index]
                                        )
                                    {
                                        ioInverterFaultChange = true;
                                    }

                                    if (index == (int)IOMachineSensors.MushroomEmergencyButtonBay1
                                        || index == (int)IOMachineSensors.MicroCarterLeftSideBay1
                                        || index == (int)IOMachineSensors.MicroCarterRightSideBay1
                                        || index == (int)IOMachineSensors.AntiIntrusionBarrierBay1
                                        || index == (int)IOMachineSensors.AntiIntrusionBarrier2Bay1)
                                    {
                                        ioSecurityChange = true;
                                    }
                                }
                            }

                            if (requiredUpdate)
                            {
                                this.logger.LogDebug($"RunningState {ioRunningStateChange}, InverterFault {ioInverterFaultChange}, security {ioSecurityChange}, ioIndex {ioIndex}, enable {this.enableNotificatons}," +
                                    $" value: {(newSensorStatus[0] ? 1 : 0)}{(newSensorStatus[1] ? 1 : 0)}{(newSensorStatus[2] ? 1 : 0)}{(newSensorStatus[3] ? 1 : 0)}{(newSensorStatus[4] ? 1 : 0)}{(newSensorStatus[5] ? 1 : 0)}{(newSensorStatus[6] ? 1 : 0)}{(newSensorStatus[7] ? 1 : 0)}," +
                                    $" {(newSensorStatus[8] ? 1 : 0)}{(newSensorStatus[9] ? 1 : 0)}{(newSensorStatus[10] ? 1 : 0)}{(newSensorStatus[11] ? 1 : 0)}{(newSensorStatus[12] ? 1 : 0)}{(newSensorStatus[13] ? 1 : 0)}{(newSensorStatus[14] ? 1 : 0)}{(newSensorStatus[15] ? 1 : 0)}");

                                Array.Copy(newSensorStatus, 0, this.sensorStatus, (ioIndex * REMOTEIO_INPUTS), REMOTEIO_INPUTS);

                                if (ioIndex == 0)
                                {
                                    var isFireAlarmActive = this.IsFireAlarmActive();
                                    if (isFireAlarmActive && newSensorStatus[(int)IOMachineSensors.RobotOptionBay1]) //FireAlarm
                                    {
                                        using (var scope = this.serviceScopeFactory.CreateScope())
                                        {
                                            var errorProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                                            var current = errorProvider.GetCurrent();
                                            if (current?.Code != (int)MachineErrorCode.FireAlarm)
                                            {
                                                if (current?.Code == (int)MachineErrorCode.PreFireAlarm)
                                                {
                                                    errorProvider.Resolve((int)current?.Id, force: true);
                                                }
                                                errorProvider.RecordNew(MachineErrorCode.FireAlarm);
                                            }
                                        }

                                        if (this.machineVolatileDataProvider.MachinePowerState > MachinePowerState.Unpowered)
                                        {
                                            var args = new StatusUpdateEventArgs();
                                            args.NewState = false;
                                            this.OnRunningStateChanged(args);
                                        }
                                    }

                                    if (isFireAlarmActive && newSensorStatus[(int)IOMachineSensors.TrolleyOptionBay1] && !newSensorStatus[(int)IOMachineSensors.RobotOptionBay1]) //PreFireAlarm
                                    {
                                        using (var scope = this.serviceScopeFactory.CreateScope())
                                        {
                                            var errorProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                                            var current = errorProvider.GetCurrent();
                                            if (current?.Code != (int)MachineErrorCode.PreFireAlarm)
                                            {
                                                errorProvider.RecordNew(MachineErrorCode.PreFireAlarm);

                                                if (this.machineVolatileDataProvider.Mode == MachineMode.Manual ||
                                                    this.machineVolatileDataProvider.Mode == MachineMode.Manual2 ||
                                                    this.machineVolatileDataProvider.Mode == MachineMode.Manual3)
                                                {
                                                    this.logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
                                                }
                                                else
                                                {
                                                    this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(BayNumber.All);
                                                    this.logger.LogInformation($"Machine status switched to {MachineMode.Manual}");
                                                }
                                            }
                                        }
                                    }

                                    if (this.enableNotificatons
                                        && ioRunningStateChange
                                        )
                                    {
                                        //During Fault Handling running status will be set off. This prevents double firing the power off procedure
                                        if (!this.IsInverterInFault)
                                        {
                                            var args = new StatusUpdateEventArgs();
                                            args.NewState = newSensorStatus[(int)IOMachineSensors.RunningState];
                                            this.OnRunningStateChanged(args);
                                        }
                                    }

                                    if (//this.enableNotificatons
                                        //&&
                                        ioInverterFaultChange
                                        )
                                    {
                                        var args = new StatusUpdateEventArgs();
                                        args.NewState = true;
                                        this.OnFaultStateChanged(args);
                                    }
                                }

                                if (!this.sensorStatus[(int)IOMachineSensors.RunningState]
                                    && ioSecurityChange
                                    )
                                {
                                    var args = new StatusUpdateEventArgs();
                                    args.NewState = false;
                                    this.OnSecurityStateChanged(args);
                                }

                                this.IsTeleOk();

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
                                if (newSensorStatus[(int)InverterSensors.ANG_ElevatorOverrunSensor]
                                    //&& this.sensorStatus[(int)IOMachineSensors.ElevatorOverrun] != newSensorStatus[(int)InverterSensors.ANG_ElevatorOverrunSensor]
                                    && this.digitalDevicesDataProvider.GetInverterTypeByIndex((InverterIndex)ioIndex) == InverterType.Ang
                                    )
                                {
                                    var errorCode = (newSensorStatus[(int)InverterSensors.ANG_ZeroElevatorSensor]) ? MachineErrorCode.ElevatorUnderrunDetected : MachineErrorCode.ElevatorOverrunDetected;
                                    using (var scope = this.serviceScopeFactory.CreateScope())
                                    {
                                        var errorProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                                        var current = errorProvider.GetCurrent();
                                        if (current == null || current.Code != (int)errorCode)
                                        {
                                            errorProvider.RecordNew(errorCode);
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

        protected virtual void OnSecurityStateChanged(StatusUpdateEventArgs e)
        {
            var handler = this.SecurityStateChanged;
            handler?.Invoke(this, e);
        }

        private bool IsFireAlarmActive()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                return machineProvider.IsFireAlarmActive();
            }
        }

        private void IsTeleOk()
        {
            using (var scope = this.serviceScopeFactory.CreateScope())
            {
                var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
                var eventAggregator = scope.ServiceProvider.GetRequiredService<IEventAggregator>();
                var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
                foreach (var bay in baysDataProvider.GetAll())
                {
                    var isError = false;
                    switch (bay.Number)
                    {
                        case BayNumber.BayOne:
                            isError = (bay.IsTelescopic &&
                                !this.TeleOkBay1);
                            break;

                        case BayNumber.BayTwo:
                            isError = (bay.IsTelescopic &&
                                !this.TeleOkBay2);
                            break;

                        case BayNumber.BayThree:
                            isError = (bay.IsTelescopic &&
                                !this.TeleOkBay3);
                            break;
                    }

                    if (isError)
                    {
                        errorsProvider.RecordNew(MachineErrorCode.TelescopicBayError, bay.Number);
                        if (this.IsMachineInRunningState)
                        {
                            var stopMachineData = new ChangeRunningStateMessageData(false, null, CommandAction.Start, StopRequestReason.Stop);
                            var stopMachineMessage = new CommandMessage(stopMachineData,
                                "Positioning OperationError",
                                MessageActor.MachineManager,
                                MessageActor.DeviceManager,
                                MessageType.ChangeRunningState,
                                bay.Number);
                            eventAggregator.GetEvent<CommandEvent>().Publish(stopMachineMessage);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
