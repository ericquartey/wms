using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        FreeSensor1 = 5,

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

        FreeSensor = 7,

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

        private List<BitModel> outputs;

        #endregion

        #region Constructors

        public IODeviceModel()
        {
            this.inputs.Add(new BitModel("Bit 00", false));
            this.inputs.Add(new BitModel("Bit 01", false));
            this.inputs.Add(new BitModel("Bit 02", false));
            this.inputs.Add(new BitModel("Bit 03", false));
            this.inputs.Add(new BitModel("Bit 04", false));
            this.inputs.Add(new BitModel("Bit 05", false));
            this.inputs.Add(new BitModel("Bit 06", false));
            this.inputs.Add(new BitModel("Bit 07", false));
            this.inputs.Add(new BitModel("Bit 08", false));
            this.inputs.Add(new BitModel("Bit 09", false));
            this.inputs.Add(new BitModel("Bit 10", false));
            this.inputs.Add(new BitModel("Bit 11", false));
            this.inputs.Add(new BitModel("Bit 12", false));
            this.inputs.Add(new BitModel("Bit 13", false));
            this.inputs.Add(new BitModel("Bit 14", false));
            this.inputs.Add(new BitModel("Bit 15", false));

            //this.inputs.PropertyChanged += (s, e) =>
            //{
            //    this.RaisePropertyChanged(nameof(this.InputsValue));
            //};
        }

        #endregion

        #region Properties

        public bool Enabled { get => this.enabled; set => this.SetProperty(ref this.enabled, value, () => this.RaisePropertyChanged(nameof(this.Enabled))); }

        public byte FirmwareVersion { get; set; } = 0x10;

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
                for (int i = 0; i < this.Inputs.Count; i++)
                {
                    if (this.Inputs[i].Value)
                    {
                        result += (ushort)Math.Pow(2, i);
                    }
                }
                return result;
            }
        }

        public List<BitModel> Outputs
        {
            get => this.outputs;
            set => this.SetProperty(ref this.outputs, value);
        }

        #endregion
    }
}
