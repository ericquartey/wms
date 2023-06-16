using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class BaseSensorsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly int OUT_PER_DEVICE = 8;

        private readonly Sensors sensors = new Sensors();

        private bool bay1HasShutter;

        private bool bay1ZeroChainIsVisible;

        private bool bay1ZeroChainUpIsVisible;

        private bool bay2HasShutter;

        private bool bay2ZeroChainIsVisible;

        private bool bay2ZeroChainUpIsVisible;

        private bool bay3HasShutter;

        private bool bay3ZeroChainIsVisible;

        private bool bay3ZeroChainUpIsVisible;

        private List<int> diagOutCurrent;

        private string diagOutCurrent0;

        private string diagOutCurrent1;

        private string diagOutCurrent10;

        private string diagOutCurrent11;

        private string diagOutCurrent12;

        private string diagOutCurrent13;

        private string diagOutCurrent14;

        private string diagOutCurrent15;

        private string diagOutCurrent16;

        private string diagOutCurrent17;

        private string diagOutCurrent18;

        private string diagOutCurrent19;

        private string diagOutCurrent2;

        private string diagOutCurrent20;

        private string diagOutCurrent21;

        private string diagOutCurrent22;

        private string diagOutCurrent23;

        private string diagOutCurrent3;

        private string diagOutCurrent4;

        private string diagOutCurrent5;

        private string diagOutCurrent6;

        private string diagOutCurrent7;

        private string diagOutCurrent8;

        private string diagOutCurrent9;

        private List<bool> diagOutFault;

        private bool diagOutFault0;

        private bool diagOutFault1;

        private bool diagOutFault10;

        private bool diagOutFault11;

        private bool diagOutFault12;

        private bool diagOutFault13;

        private bool diagOutFault14;

        private bool diagOutFault15;

        private bool diagOutFault16;

        private bool diagOutFault17;

        private bool diagOutFault18;

        private bool diagOutFault19;

        private bool diagOutFault2;

        private bool diagOutFault20;

        private bool diagOutFault21;

        private bool diagOutFault22;

        private bool diagOutFault23;

        private bool diagOutFault3;

        private bool diagOutFault4;

        private bool diagOutFault5;

        private bool diagOutFault6;

        private bool diagOutFault7;

        private bool diagOutFault8;

        private bool diagOutFault9;

        private SubscriptionToken diagOutToken;

        private bool isBay1ExternalDoublePresent;

        private bool isBay1ExternalPresent;

        private bool isBay1InternalPresent;

        private bool isBay1PositionDownPresent;

        private bool isBay1PositionUpPresent;

        private bool isBay1TelescopicPresent;

        private bool isBay2ExternalDoublePresent;

        private bool isBay2ExternalPresent;

        private bool isBay2InternalPresent;

        private bool isBay2PositionDownPresent;

        private bool isBay2PositionUpPresent;

        private bool isBay2Present;

        private bool isBay2TelescopicPresent;

        private bool isBay3ExternalDoublePresent;

        private bool isBay3ExternalPresent;

        private bool isBay3InternalPresent;

        private bool isBay3PositionDownPresent;

        private bool isBay3PositionUpPresent;

        private bool isBay3Present;

        private bool isBay3TelescopicPresent;

        private bool isFireAlarmActive;

        private bool isSpeaActive;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseSensorsViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager,
            IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new System.ArgumentNullException(nameof(machineIdentityWebService));

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public bool Bay1HasShutter { get => this.bay1HasShutter; private set => this.SetProperty(ref this.bay1HasShutter, value); }

        public bool Bay1ZeroChainIsVisible { get => this.bay1ZeroChainIsVisible; private set => this.SetProperty(ref this.bay1ZeroChainIsVisible, value); }

        public bool Bay1ZeroChainUpIsVisible { get => this.bay1ZeroChainUpIsVisible; private set => this.SetProperty(ref this.bay1ZeroChainUpIsVisible, value); }

        public bool Bay2HasShutter { get => this.bay2HasShutter; private set => this.SetProperty(ref this.bay2HasShutter, value); }

        public bool Bay2ZeroChainIsVisible { get => this.bay2ZeroChainIsVisible; private set => this.SetProperty(ref this.bay2ZeroChainIsVisible, value); }

        public bool Bay2ZeroChainUpIsVisible { get => this.bay2ZeroChainUpIsVisible; private set => this.SetProperty(ref this.bay2ZeroChainUpIsVisible, value); }

        public bool Bay3HasShutter { get => this.bay3HasShutter; private set => this.SetProperty(ref this.bay3HasShutter, value); }

        public bool Bay3ZeroChainIsVisible { get => this.bay3ZeroChainIsVisible; private set => this.SetProperty(ref this.bay3ZeroChainIsVisible, value); }

        public bool Bay3ZeroChainUpIsVisible { get => this.bay3ZeroChainUpIsVisible; private set => this.SetProperty(ref this.bay3ZeroChainUpIsVisible, value); }

        public List<int> DiagOutCurrent { get => this.diagOutCurrent; private set => this.SetProperty(ref this.diagOutCurrent, value); }

        //  0 a 65536
        public string DiagOutCurrent0 { get => this.diagOutCurrent0; set => this.SetProperty(ref this.diagOutCurrent0, value); }

        public string DiagOutCurrent1 { get => this.diagOutCurrent1; set => this.SetProperty(ref this.diagOutCurrent1, value); }

        public string DiagOutCurrent10 { get => this.diagOutCurrent10; set => this.SetProperty(ref this.diagOutCurrent10, value); }

        public string DiagOutCurrent11 { get => this.diagOutCurrent11; set => this.SetProperty(ref this.diagOutCurrent11, value); }

        public string DiagOutCurrent12 { get => this.diagOutCurrent12; set => this.SetProperty(ref this.diagOutCurrent12, value); }

        public string DiagOutCurrent13 { get => this.diagOutCurrent13; set => this.SetProperty(ref this.diagOutCurrent13, value); }

        public string DiagOutCurrent14 { get => this.diagOutCurrent14; set => this.SetProperty(ref this.diagOutCurrent14, value); }

        public string DiagOutCurrent15 { get => this.diagOutCurrent15; set => this.SetProperty(ref this.diagOutCurrent15, value); }

        public string DiagOutCurrent16 { get => this.diagOutCurrent16; set => this.SetProperty(ref this.diagOutCurrent16, value); }

        public string DiagOutCurrent17 { get => this.diagOutCurrent17; set => this.SetProperty(ref this.diagOutCurrent17, value); }

        public string DiagOutCurrent18 { get => this.diagOutCurrent18; set => this.SetProperty(ref this.diagOutCurrent18, value); }

        public string DiagOutCurrent19 { get => this.diagOutCurrent19; set => this.SetProperty(ref this.diagOutCurrent19, value); }

        public string DiagOutCurrent2 { get => this.diagOutCurrent2; set => this.SetProperty(ref this.diagOutCurrent2, value); }

        public string DiagOutCurrent20 { get => this.diagOutCurrent20; set => this.SetProperty(ref this.diagOutCurrent20, value); }

        public string DiagOutCurrent21 { get => this.diagOutCurrent21; set => this.SetProperty(ref this.diagOutCurrent21, value); }

        public string DiagOutCurrent22 { get => this.diagOutCurrent22; set => this.SetProperty(ref this.diagOutCurrent22, value); }

        public string DiagOutCurrent23 { get => this.diagOutCurrent23; set => this.SetProperty(ref this.diagOutCurrent23, value); }

        public string DiagOutCurrent3 { get => this.diagOutCurrent3; set => this.SetProperty(ref this.diagOutCurrent3, value); }

        public string DiagOutCurrent4 { get => this.diagOutCurrent4; set => this.SetProperty(ref this.diagOutCurrent4, value); }

        public string DiagOutCurrent5 { get => this.diagOutCurrent5; set => this.SetProperty(ref this.diagOutCurrent5, value); }

        public string DiagOutCurrent6 { get => this.diagOutCurrent6; set => this.SetProperty(ref this.diagOutCurrent6, value); }

        public string DiagOutCurrent7 { get => this.diagOutCurrent7; set => this.SetProperty(ref this.diagOutCurrent7, value); }

        public string DiagOutCurrent8 { get => this.diagOutCurrent8; set => this.SetProperty(ref this.diagOutCurrent8, value); }

        public string DiagOutCurrent9 { get => this.diagOutCurrent9; set => this.SetProperty(ref this.diagOutCurrent9, value); }

        public List<bool> DiagOutFault { get => this.diagOutFault; private set => this.SetProperty(ref this.diagOutFault, value); }

        public bool DiagOutFault0 { get => this.diagOutFault0; set => this.SetProperty(ref this.diagOutFault0, value); }

        public bool DiagOutFault1 { get => this.diagOutFault1; set => this.SetProperty(ref this.diagOutFault1, value); }

        public bool DiagOutFault10 { get => this.diagOutFault10; set => this.SetProperty(ref this.diagOutFault10, value); }

        public bool DiagOutFault11 { get => this.diagOutFault11; set => this.SetProperty(ref this.diagOutFault11, value); }

        public bool DiagOutFault12 { get => this.diagOutFault12; set => this.SetProperty(ref this.diagOutFault12, value); }

        public bool DiagOutFault13 { get => this.diagOutFault13; set => this.SetProperty(ref this.diagOutFault13, value); }

        public bool DiagOutFault14 { get => this.diagOutFault14; set => this.SetProperty(ref this.diagOutFault14, value); }

        public bool DiagOutFault15 { get => this.diagOutFault15; set => this.SetProperty(ref this.diagOutFault15, value); }

        public bool DiagOutFault16 { get => this.diagOutFault16; set => this.SetProperty(ref this.diagOutFault16, value); }

        public bool DiagOutFault17 { get => this.diagOutFault17; set => this.SetProperty(ref this.diagOutFault17, value); }

        public bool DiagOutFault18 { get => this.diagOutFault18; set => this.SetProperty(ref this.diagOutFault18, value); }

        public bool DiagOutFault19 { get => this.diagOutFault19; set => this.SetProperty(ref this.diagOutFault19, value); }

        public bool DiagOutFault2 { get => this.diagOutFault2; set => this.SetProperty(ref this.diagOutFault2, value); }

        public bool DiagOutFault20 { get => this.diagOutFault20; set => this.SetProperty(ref this.diagOutFault20, value); }

        public bool DiagOutFault21 { get => this.diagOutFault21; set => this.SetProperty(ref this.diagOutFault21, value); }

        public bool DiagOutFault22 { get => this.diagOutFault22; set => this.SetProperty(ref this.diagOutFault22, value); }

        public bool DiagOutFault23 { get => this.diagOutFault23; set => this.SetProperty(ref this.diagOutFault23, value); }

        public bool DiagOutFault3 { get => this.diagOutFault3; set => this.SetProperty(ref this.diagOutFault3, value); }

        public bool DiagOutFault4 { get => this.diagOutFault4; set => this.SetProperty(ref this.diagOutFault4, value); }

        public bool DiagOutFault5 { get => this.diagOutFault5; set => this.SetProperty(ref this.diagOutFault5, value); }

        public bool DiagOutFault6 { get => this.diagOutFault6; set => this.SetProperty(ref this.diagOutFault6, value); }

        public bool DiagOutFault7 { get => this.diagOutFault7; set => this.SetProperty(ref this.diagOutFault7, value); }

        public bool DiagOutFault8 { get => this.diagOutFault8; set => this.SetProperty(ref this.diagOutFault8, value); }

        public bool DiagOutFault9 { get => this.diagOutFault9; set => this.SetProperty(ref this.diagOutFault9, value); }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBay1ExternalDoublePresent { get => this.isBay1ExternalDoublePresent; private set => this.SetProperty(ref this.isBay1ExternalDoublePresent, value); }

        public bool IsBay1ExternalPresent { get => this.isBay1ExternalPresent; private set => this.SetProperty(ref this.isBay1ExternalPresent, value); }

        public bool IsBay1InternalPresent { get => this.isBay1InternalPresent; private set => this.SetProperty(ref this.isBay1InternalPresent, value); }

        public bool IsBay1PositionDownPresent { get => this.isBay1PositionDownPresent; private set => this.SetProperty(ref this.isBay1PositionDownPresent, value); }

        public bool IsBay1PositionUpPresent { get => this.isBay1PositionUpPresent; private set => this.SetProperty(ref this.isBay1PositionUpPresent, value); }

        public bool IsBay1TelescopicPresent { get => this.isBay1TelescopicPresent; private set => this.SetProperty(ref this.isBay1TelescopicPresent, value); }

        public bool IsBay2ExternalDoublePresent { get => this.isBay2ExternalDoublePresent; private set => this.SetProperty(ref this.isBay2ExternalDoublePresent, value); }

        public bool IsBay2ExternalPresent { get => this.isBay2ExternalPresent; private set => this.SetProperty(ref this.isBay2ExternalPresent, value); }

        public bool IsBay2InternalPresent { get => this.isBay2InternalPresent; private set => this.SetProperty(ref this.isBay2InternalPresent, value); }

        public bool IsBay2PositionDownPresent { get => this.isBay2PositionDownPresent; private set => this.SetProperty(ref this.isBay2PositionDownPresent, value); }

        public bool IsBay2PositionUpPresent { get => this.isBay2PositionUpPresent; private set => this.SetProperty(ref this.isBay2PositionUpPresent, value); }

        public bool IsBay2Present { get => this.isBay2Present; private set => this.SetProperty(ref this.isBay2Present, value); }

        public bool IsBay2TelescopicPresent { get => this.isBay2TelescopicPresent; private set => this.SetProperty(ref this.isBay2TelescopicPresent, value); }

        public bool IsBay3ExternalDoublePresent { get => this.isBay3ExternalDoublePresent; private set => this.SetProperty(ref this.isBay3ExternalDoublePresent, value); }

        public bool IsBay3ExternalPresent { get => this.isBay3ExternalPresent; private set => this.SetProperty(ref this.isBay3ExternalPresent, value); }

        public bool IsBay3InternalPresent { get => this.isBay3InternalPresent; private set => this.SetProperty(ref this.isBay3InternalPresent, value); }

        public bool IsBay3PositionDownPresent { get => this.isBay3PositionDownPresent; private set => this.SetProperty(ref this.isBay3PositionDownPresent, value); }

        public bool IsBay3PositionUpPresent { get => this.isBay3PositionUpPresent; private set => this.SetProperty(ref this.isBay3PositionUpPresent, value); }

        public bool IsBay3Present { get => this.isBay3Present; private set => this.SetProperty(ref this.isBay3Present, value); }

        public bool IsBay3TelescopicPresent { get => this.isBay3TelescopicPresent; private set => this.SetProperty(ref this.isBay3TelescopicPresent, value); }

        public bool IsFireAlarmActive { get => this.isFireAlarmActive; private set => this.SetProperty(ref this.isFireAlarmActive, value); }

        public bool IsOneTonMachine => this.bayManager.Identity.IsOneTonMachine;

        public bool IsSpeaActive { get => this.isSpeaActive; private set => this.SetProperty(ref this.isSpeaActive, value); }

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public int OutPerDevice => this.OUT_PER_DEVICE;

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            /*
             * Avoid unsubscribing in case of navigation to error page.
             * We may need to review this behaviour.
             *
            this.subscriptionToken?.Dispose();
            this.subscriptionToken = null;
            */
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;
            this.DiagOutCurrent = new List<int>();
            this.DiagOutFault = new List<bool>();
            for (int i = 0; i < this.OUT_PER_DEVICE * 3; i++)
            {
                this.DiagOutFault.Add(false);
                this.DiagOutCurrent.Add(0);
            }

            this.SubscribeToEvents();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            var sensorsStates = await this.machineSensorsWebService.GetAsync();

            var bays = await this.machineBaysWebService.GetAllAsync();

            this.IsBay2Present = bays.Any(b => b.Number == BayNumber.BayTwo);
            this.IsBay3Present = bays.Any(b => b.Number == BayNumber.BayThree);

            this.Bay1HasShutter = bays
                .Where(b => b.Number == BayNumber.BayOne)
                .Select(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified)
                .SingleOrDefault();

            this.Bay2HasShutter = bays
                .Where(b => b.Number == BayNumber.BayTwo)
                .Select(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified)
                .SingleOrDefault();

            this.Bay3HasShutter = bays
                .Where(b => b.Number == BayNumber.BayThree)
                .Select(b => b.Shutter != null && b.Shutter.Type != ShutterType.NotSpecified)
                .SingleOrDefault();

            this.CheckZeroChainOnBays(bays);

            var bay1 = bays.FirstOrDefault(f => f.Number == BayNumber.BayOne);
            var bay2 = bays.FirstOrDefault(f => f.Number == BayNumber.BayTwo);
            var bay3 = bays.FirstOrDefault(f => f.Number == BayNumber.BayThree);

            this.IsBay1PositionDownPresent = (bay1?.IsDouble ?? false) || (!bay1?.Positions?.Any(o => o.IsUpper) ?? false);
            this.IsBay1PositionUpPresent = (bay1?.IsDouble ?? false) || (bay1?.Positions?.Any(o => o.IsUpper) ?? false);

            if (!(bay1 is null))
            {
                this.IsBay1ExternalPresent = bay1.IsExternal && !bay1.IsDouble;
                this.IsBay1ExternalDoublePresent = bay1.IsExternal && bay1.IsDouble;
                this.IsBay1InternalPresent = !this.IsBay1ExternalPresent && !this.IsBay1ExternalDoublePresent;
                this.IsBay1TelescopicPresent = bay1.IsTelescopic;
            }
            if (!(bay2 is null))
            {
                this.IsBay2ExternalPresent = bay2.IsExternal && !bay2.IsDouble;
                this.IsBay2ExternalDoublePresent = bay2.IsExternal && bay2.IsDouble;
                this.IsBay2InternalPresent = !this.IsBay2ExternalPresent && !this.IsBay2ExternalDoublePresent;
                this.IsBay2TelescopicPresent = bay2.IsTelescopic;
            }
            if (!(bay3 is null))
            {
                this.IsBay3ExternalPresent = bay3.IsExternal && !bay3.IsDouble;
                this.IsBay3ExternalDoublePresent = bay3.IsExternal && bay3.IsDouble;
                this.IsBay3InternalPresent = !this.IsBay3ExternalPresent && !this.IsBay3ExternalDoublePresent;
                this.IsBay3TelescopicPresent = bay3.IsTelescopic;
            }

            this.IsBay2PositionDownPresent = (bay2?.IsDouble ?? false) || (!bay2?.Positions?.Any(o => o.IsUpper) ?? false);
            this.IsBay2PositionUpPresent = (bay2?.IsDouble ?? false) || (bay2?.Positions?.Any(o => o.IsUpper) ?? false);

            this.IsBay3PositionDownPresent = (bay3?.IsDouble ?? false) || (!bay3?.Positions?.Any(o => o.IsUpper) ?? false);
            this.IsBay3PositionUpPresent = (bay3?.IsDouble ?? false) || (bay3?.Positions?.Any(o => o.IsUpper) ?? false);

            this.IsFireAlarmActive = await this.machineIdentityWebService.GetFireAlarmEnableAsync();

            this.IsSpeaActive = await this.machineIdentityWebService.GetIsSpeaEnableAsync();

            this.sensors.Update(sensorsStates.ToArray());

            var current = await this.machineSensorsWebService.GetOutCurrentAsync();
            this.DiagOutCurrent = current.ToList();

            var fault = await this.machineSensorsWebService.GetOutFaultAsync();
            this.DiagOutFault = fault.ToList();

            this.DiagOutUpdate();
        }

        private void CheckZeroChainOnBays(IEnumerable<Bay> bays)
        {
            this.Bay1ZeroChainIsVisible = bays
                .Where(b => b.Number == BayNumber.BayOne)
                .Select(b => b.Carousel != null || b.IsExternal)
                .SingleOrDefault();

            this.Bay2ZeroChainIsVisible = bays
                .Where(b => b.Number == BayNumber.BayTwo)
                .Select(b => b.Carousel != null || b.IsExternal)
                .SingleOrDefault();

            this.Bay3ZeroChainIsVisible = bays
                .Where(b => b.Number == BayNumber.BayThree)
                .Select(b => b.Carousel != null || b.IsExternal)
                .SingleOrDefault();

            this.Bay1ZeroChainUpIsVisible = bays
                .Where(b => b.Number == BayNumber.BayOne)
                .Select(b => b.IsDouble && b.IsExternal)
                .SingleOrDefault();

            this.Bay2ZeroChainUpIsVisible = bays
                .Where(b => b.Number == BayNumber.BayTwo)
                .Select(b => b.IsDouble && b.IsExternal)
                .SingleOrDefault();

            this.Bay3ZeroChainUpIsVisible = bays
                .Where(b => b.Number == BayNumber.BayThree)
                .Select(b => b.IsDouble && b.IsExternal)
                .SingleOrDefault();
        }

        private void DiagOutUpdate()
        {
            try
            {
                this.DiagOutFault0 = this.DiagOutFault[0];
                this.DiagOutFault1 = this.DiagOutFault[1];
                this.DiagOutFault2 = this.DiagOutFault[2];
                this.DiagOutFault3 = this.DiagOutFault[3];
                this.DiagOutFault4 = this.DiagOutFault[4];
                this.DiagOutFault5 = this.DiagOutFault[5];
                this.DiagOutFault6 = this.DiagOutFault[6];
                this.DiagOutFault7 = this.DiagOutFault[7];
                this.DiagOutFault8 = this.DiagOutFault[8];
                this.DiagOutFault9 = this.DiagOutFault[9];
                this.DiagOutFault10 = this.DiagOutFault[10];
                this.DiagOutFault11 = this.DiagOutFault[11];
                this.DiagOutFault12 = this.DiagOutFault[12];
                this.DiagOutFault13 = this.DiagOutFault[13];
                this.DiagOutFault14 = this.DiagOutFault[14];
                this.DiagOutFault15 = this.DiagOutFault[15];
                this.DiagOutFault16 = this.DiagOutFault[16];
                this.DiagOutFault17 = this.DiagOutFault[17];
                this.DiagOutFault18 = this.DiagOutFault[18];
                this.DiagOutFault19 = this.DiagOutFault[19];
                this.DiagOutFault20 = this.DiagOutFault[20];
                this.DiagOutFault21 = this.DiagOutFault[21];
                this.DiagOutFault22 = this.DiagOutFault[22];
                this.DiagOutFault23 = this.DiagOutFault[23];
            }
            catch (System.Exception)
            {
            }

            try
            {
                this.DiagOutCurrent0 = this.DiagOutCurrent[0].ToString();
                this.DiagOutCurrent1 = this.DiagOutCurrent[1].ToString();
                this.DiagOutCurrent2 = this.DiagOutCurrent[2].ToString();
                this.DiagOutCurrent3 = this.DiagOutCurrent[3].ToString();
                this.DiagOutCurrent4 = this.DiagOutCurrent[4].ToString();
                this.DiagOutCurrent5 = this.DiagOutCurrent[5].ToString();
                this.DiagOutCurrent6 = this.DiagOutCurrent[6].ToString();
                this.DiagOutCurrent7 = this.DiagOutCurrent[7].ToString();
                this.DiagOutCurrent8 = this.DiagOutCurrent[8].ToString();
                this.DiagOutCurrent9 = this.DiagOutCurrent[9].ToString();
                this.DiagOutCurrent10 = this.DiagOutCurrent[10].ToString();
                this.DiagOutCurrent11 = this.DiagOutCurrent[11].ToString();
                this.DiagOutCurrent12 = this.DiagOutCurrent[12].ToString();
                this.DiagOutCurrent13 = this.DiagOutCurrent[13].ToString();
                this.DiagOutCurrent14 = this.DiagOutCurrent[14].ToString();
                this.DiagOutCurrent15 = this.DiagOutCurrent[15].ToString();
                this.DiagOutCurrent16 = this.DiagOutCurrent[16].ToString();
                this.DiagOutCurrent17 = this.DiagOutCurrent[17].ToString();
                this.DiagOutCurrent18 = this.DiagOutCurrent[18].ToString();
                this.DiagOutCurrent19 = this.DiagOutCurrent[19].ToString();
                this.DiagOutCurrent20 = this.DiagOutCurrent[20].ToString();
                this.DiagOutCurrent21 = this.DiagOutCurrent[21].ToString();
                this.DiagOutCurrent22 = this.DiagOutCurrent[22].ToString();
                this.DiagOutCurrent23 = this.DiagOutCurrent[23].ToString();
            }
            catch (System.Exception)
            {
            }
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.SECURITY,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.Localized.Get("InstallationApp.Security"),
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.VERTICALAXIS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.Localized.Get("InstallationApp.VerticalAxisButton"),
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.BAYS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.Localized.Get("InstallationApp.Bays"),
                    trackCurrentView: false));

            if (this.MachineService.Bays.ToList().Exists(x => x.Number == BayNumber.BayOne))
            {
                this.menuItems.Add(
                                new NavigationMenuItem(
                                    Utils.Modules.Installation.Sensors.IO1,
                                    nameof(Utils.Modules.Installation),
                                    VW.App.Resources.Localized.Get("InstallationApp.DeviceIO1"),
                                    trackCurrentView: false));
            }

            if (this.MachineService.Bays.ToList().Exists(x => x.Number == BayNumber.BayTwo))
            {
                this.menuItems.Add(
                                new NavigationMenuItem(
                                    Utils.Modules.Installation.Sensors.IO2,
                                    nameof(Utils.Modules.Installation),
                                    VW.App.Resources.Localized.Get("InstallationApp.DeviceIO2"),
                                    trackCurrentView: false));
            }

            if (this.MachineService.Bays.ToList().Exists(x => x.Number == BayNumber.BayThree))
            {
                this.menuItems.Add(
                                new NavigationMenuItem(
                                    Utils.Modules.Installation.Sensors.IO3,
                                    nameof(Utils.Modules.Installation),
                                    VW.App.Resources.Localized.Get("InstallationApp.DeviceIO3"),
                                    trackCurrentView: false));
            }
        }

        private void OnDiagOutChanged(NotificationMessageUI<DiagOutChangedMessageData> message)
        {
            for (int i = 0; i < this.OUT_PER_DEVICE; i++)
            {
                this.DiagOutFault[message.Data.IoIndex * this.OUT_PER_DEVICE + i] = message.Data.FaultStates[i];
                this.DiagOutCurrent[message.Data.IoIndex * this.OUT_PER_DEVICE + i] = message.Data.CurrentStates[i];
            }

            this.DiagOutUpdate();
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStatesInput);
        }

        private void SubscribeToEvents()
        {
            this.subscriptionToken = this.subscriptionToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Subscribe(
                        this.OnSensorsChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.SensorsStatesInput != null);

            this.diagOutToken = this.diagOutToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<DiagOutChangedMessageData>>()
                    .Subscribe(
                        this.OnDiagOutChanged,
                        ThreadOption.UIThread,
                        false,
                        m => m.Data?.FaultStates != null);
        }

        #endregion
    }
}
