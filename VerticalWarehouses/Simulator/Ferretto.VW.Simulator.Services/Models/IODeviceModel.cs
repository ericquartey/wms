using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Simulator.Resources;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public enum IoPorts
    {
        #region Outputs

        ResetSecurity = 0,

        ElevatorMotor = 1,

        CradleMotor = 2,

        MeasureBarrier = 3,

        BayLight = 4,

        PowerEnable = 5,

        EndMissionRobot = 6,

        ReadyWarehouseRobot = 7,

        #endregion

        #region Inputs

        NormalState = 0,

        MushroomEmergency = 1,

        MicroCarterLeftSideBay = 2,

        MicroCarterRightSideBay = 3,

        AntiIntrusionBarrierBay = 4,

        LoadingUnitInBay = 5,

        LoadingUnitInLowerBay = 6,

        InverterInFault = 7,

        ElevatorMotorFeedback = 8,

        CradleMotorFeedback = 9,

        DrawerInMachineSide = 10,

        DrawerInOperatorSide = 11,

        CalibrationBarrierLight = 12,

        HookTrolley = 14,

        FinePickingRobot = 15

        #endregion
    }

    public class IODeviceModel : BindableBase
    {
        #region Fields

        public byte[] Buffer;

        private bool enabled = true;

        private ObservableCollection<BitModel> inputs = new ObservableCollection<BitModel>();

        private Machine machine;

        private List<BitModel> outputs;

        #endregion

        #region Constructors

        public IODeviceModel()
        {
            // Initialize inputs
            this.inputs.Add(new BitModel("00", false, IODevice.SafetyStatus));
            this.inputs.Add(new BitModel("01", false, IODevice.EmergencyPB));
            this.inputs.Add(new BitModel("02", false, IODevice.CarterSensorLeft));
            this.inputs.Add(new BitModel("03", false, IODevice.CarterSensorRight));
            this.inputs.Add(new BitModel("04", false, IODevice.LightCurtain));
            this.inputs.Add(new BitModel("05", false, IODevice.UpperBayBoxSensorPresence));
            this.inputs.Add(new BitModel("06", false, IODevice.LowerBayBoxSensorPresence));
            this.inputs.Add(new BitModel("07", false, IODevice.InverterFaultSignal));
            this.inputs.Add(new BitModel("08", false, IODevice.FeedbackElevatorMotorSelected));
            this.inputs.Add(new BitModel("09", false, IODevice.FeedbackElevatorChainMotorSelected));
            this.inputs.Add(new BitModel("10", false, IODevice.OperatorSideElevatorChainBoxPresence));
            this.inputs.Add(new BitModel("11", false, IODevice.MachineSideElevatorChainBoxPresence));
            this.inputs.Add(new BitModel("12", false, IODevice.LightCurtainCalibration));
            this.inputs.Add(new BitModel("13", false, IODevice.InnerLightCurtain));
            this.inputs.Add(new BitModel("14", false, IODevice.TrolleyHook));
            this.inputs.Add(new BitModel("15", false, IODevice.RobotEndPicking));

            // Initialize ouputs
            this.outputs = Enumerable.Range(0, 8).Select(x => new BitModel($"{x}", false, GetRemoteIOSignalDescription(x))).ToList();

            // Remove emergency button
            this.Inputs[(int)IoPorts.MushroomEmergency].Value = true;
            this.Inputs[(int)IoPorts.MicroCarterLeftSideBay].Value = true;
            this.Inputs[(int)IoPorts.MicroCarterRightSideBay].Value = true;
            this.Inputs[(int)IoPorts.AntiIntrusionBarrierBay].Value = true;

            // Set empty position on bay
            this.Inputs[(int)IoPorts.LoadingUnitInBay].Value = true;
            this.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = true;
        }

        #endregion

        #region Properties

        public bool Enabled { get => this.enabled; set => this.SetProperty(ref this.enabled, value); }

        public byte FirmwareVersion { get; set; } = 0x11;

        public int Id { get; set; }

        public ObservableCollection<BitModel> Inputs
        {
            get => this.inputs;
            set => this.SetProperty(ref this.inputs, value);
        }

        public ushort InputsValue
        {
            get
            {
                ushort result = 0;
                for (var i = 0; i < this.Inputs.Count; i++)
                {
                    if (this.Inputs[i].Value)
                    {
                        result += (ushort)Math.Pow(2, i);
                    }
                }
                return result;
            }
        }

        public Machine Machine
        {
            get { return this.machine; }
            set
            {
                this.machine = value;

                if (this.Machine != null)
                {
                    var bay = this.Machine.Bays.FirstOrDefault(x => (int)x.Number == this.Id + 1);
                    if (bay != null)
                    {
                        var hasCarousel = bay.Carousel != null;
                        var hasExternal = bay.External != null;

                        this.Enabled = true;

                        // Set empty position on bay, according to the bay type
                        if (hasCarousel)
                        {
                            this.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = false;
                        }
                        if (hasExternal)
                        {
                            this.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = false;
                        }
                        if (!hasCarousel && !hasExternal)
                        {
                            this.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = true;
                        }
                        if (hasExternal && bay.IsDouble)
                        {
                            this.Inputs[(int)IoPorts.LoadingUnitInLowerBay].Value = true;
                        }
                    }
                    else
                    {
                        this.Enabled = false;
                    }
                }
            }
        }

        public List<BitModel> Outputs
        {
            get => this.outputs;
            set => this.SetProperty(ref this.outputs, value);
        }

        #endregion

        #region Methods

        internal static string GetRemoteIOSignalDescription(int signalIndex)
        {
            switch (signalIndex)
            {
                case 0:
                    return IODevice.ResetSafetyFunction;

                case 1:
                    return IODevice.MotorElevatorSelection;

                case 2:
                    return IODevice.MotorElevatorChainSelection;

                case 3:
                    return IODevice.HeightReadingActivation;

                case 4:
                    return IODevice.BayLightActivation;

                case 5:
                    return IODevice.EnableRunConsole;

                case 6:
                    return IODevice.RobotOptionMissionCompletedWarhouse;

                case 7:
                    return IODevice.RobotOptionWarhouseReadyFault;

                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}
