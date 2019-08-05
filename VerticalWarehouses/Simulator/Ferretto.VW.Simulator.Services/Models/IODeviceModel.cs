using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Simulator.Services.Helpers;
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

        private ObservableCollectionWithItemNotify<BitModel> ios = new ObservableCollectionWithItemNotify<BitModel>();

        #endregion

        #region Constructors

        public IODeviceModel()
        {
            this.ios.Add(new BitModel("Id:00", false));
            this.ios.Add(new BitModel("Id:01", false));
            this.ios.Add(new BitModel("Id:02", false));
            this.ios.Add(new BitModel("Id:03", false));
            this.ios.Add(new BitModel("Id:04", false));
            this.ios.Add(new BitModel("Id:05", false));
            this.ios.Add(new BitModel("Id:06", false));
            this.ios.Add(new BitModel("Id:07", false));
            this.ios.Add(new BitModel("Id:08", false));
            this.ios.Add(new BitModel("Id:09", false));
            this.ios.Add(new BitModel("Id:10", false));
            this.ios.Add(new BitModel("Id:11", false));
            this.ios.Add(new BitModel("Id:12", false));
            this.ios.Add(new BitModel("Id:13", false));
            this.ios.Add(new BitModel("Id:14", false));
            this.ios.Add(new BitModel("Id:15", false));

            this.ios.PropertyChanged += (s, e) =>
            {
                this.RaisePropertyChanged(nameof(this.IOValue));
            };
        }

        #endregion

        #region Properties

        public bool Enabled { get => this.enabled; set => this.SetProperty(ref this.enabled, value, () => this.RaisePropertyChanged(nameof(this.Enabled))); }

        public byte FirmwareVersion { get; set; } = 0x10;

        public int Id { get; set; }

        public ObservableCollectionWithItemNotify<BitModel> IOs
        {
            get => this.ios;
            set => this.SetProperty(ref this.ios, value);
        }

        public ushort IOValue
        {
            get
            {
                ushort result = 0;
                for (int i = 0; i < this.IOs.Count; i++)
                {
                    if (this.IOs[i].Value)
                    {
                        result += (ushort)Math.Pow(2, i);
                    }
                }
                return result;
            }
        }

        #endregion
    }
}
