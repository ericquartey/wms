using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
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

        private List<BitModel> outputs;

        #endregion

        #region Constructors

        public IODeviceModel()
        {
            // Initialize inputs
            this.inputs.Add(new BitModel("00", false, "Stato funzione sicurezza"));
            this.inputs.Add(new BitModel("01", false, "Fungo di emergenza"));
            this.inputs.Add(new BitModel("02", false, "Sensore carter protezione SX"));
            this.inputs.Add(new BitModel("03", false, "Sensore carter protezione DX"));
            this.inputs.Add(new BitModel("04", false, "Barriera ottica anti-intrusione"));
            this.inputs.Add(new BitModel("05", false, "Sensore presenza cassetto in baia"));
            this.inputs.Add(new BitModel("06", false, "Sensore presenza cassetto inferiore / baia intermedia"));
            this.inputs.Add(new BitModel("07", false, "Cumulativo Inverter In Fault"));
            this.inputs.Add(new BitModel("08", false, "Selezione motore elevatore (feedback)"));
            this.inputs.Add(new BitModel("09", false, "Selezione motore culla (feedback)"));
            this.inputs.Add(new BitModel("10", false, "Presenza cassetto su culla lato operatore"));
            this.inputs.Add(new BitModel("11", false, "Presenza cassetto su culla lato macchina"));
            this.inputs.Add(new BitModel("12", false, "Taratura barriera"));
            this.inputs.Add(new BitModel("13", false, "Libero"));
            this.inputs.Add(new BitModel("14", false, "Opzione trolley - Aggancio trolley"));
            this.inputs.Add(new BitModel("15", false, "Opzione robot - Tasto fine picking (oppure fine picking robot)"));

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

        public Machine Machine { get; set; }

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
                    return "Reset funzione sicurezza (impulso 300ms)";

                case 1:
                    return "Selezione motore elevatore";

                case 2:
                    return "Selezione motore culla";

                case 3:
                    return "Attivazione lettura altezza (barriera di misura)";

                case 4:
                    return "Attivazione illuminazione baia";

                case 5:
                    return "Abilitazione al marcia dalla console";

                case 6:
                    return "Opzione robot - Missione terminata magazzino";

                case 7:
                    return "Opzione robot - Magazzino pronto/guasto";

                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}
