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

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly Sensors sensors = new Sensors();

        private bool bay1HasShutter;

        private bool bay1ZeroChainisVisible;

        private bool bay2HasShutter;

        private bool bay2ZeroChainIsVisible;

        private bool bay3HasShutter;

        private bool bay3ZeroChainIsVisible;

        private bool isBay1ExternalDoublePresent;

        private bool isBay1ExternalPresent;

        private bool isBay1InternalPresent;

        private bool isBay1PositionDownPresent;

        private bool isBay1PositionUpPresent;

        private bool isBay2ExternalDoublePresent;

        private bool isBay2ExternalPresent;

        private bool isBay2InternalPresent;

        private bool isBay2PositionDownPresent;

        private bool isBay2PositionUpPresent;

        private bool isBay2Present;

        private bool isBay3ExternalDoublePresent;

        private bool isBay3ExternalPresent;

        private bool isBay3InternalPresent;

        private bool isBay3PositionDownPresent;

        private bool isBay3PositionUpPresent;

        private bool isBay3Present;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseSensorsViewModel(
            IMachineSensorsWebService machineSensorsWebService,
            IMachineBaysWebService machineBaysWebService,
            IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.machineSensorsWebService = machineSensorsWebService ?? throw new System.ArgumentNullException(nameof(machineSensorsWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));
            this.bayManager = bayManager ?? throw new System.ArgumentNullException(nameof(bayManager));

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public bool Bay1HasShutter { get => this.bay1HasShutter; private set => this.SetProperty(ref this.bay1HasShutter, value); }

        public bool Bay1ZeroChainIsVisible { get => this.bay1ZeroChainisVisible; private set => this.SetProperty(ref this.bay1ZeroChainisVisible, value); }

        public bool Bay2HasShutter { get => this.bay2HasShutter; private set => this.SetProperty(ref this.bay2HasShutter, value); }

        public bool Bay2ZeroChainIsVisible { get => this.bay2ZeroChainIsVisible; private set => this.SetProperty(ref this.bay2ZeroChainIsVisible, value); }

        public bool Bay3HasShutter { get => this.bay3HasShutter; private set => this.SetProperty(ref this.bay3HasShutter, value); }

        public bool Bay3ZeroChainIsVisible { get => this.bay3ZeroChainIsVisible; private set => this.SetProperty(ref this.bay3ZeroChainIsVisible, value); }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBay1ExternalDoublePresent { get => this.isBay1ExternalDoublePresent; private set => this.SetProperty(ref this.isBay1ExternalDoublePresent, value); }

        public bool IsBay1ExternalPresent { get => this.isBay1ExternalPresent; private set => this.SetProperty(ref this.isBay1ExternalPresent, value); }

        public bool IsBay1InternalPresent { get => this.isBay1InternalPresent; private set => this.SetProperty(ref this.isBay1InternalPresent, value); }

        public bool IsBay1PositionDownPresent { get => this.isBay1PositionDownPresent; private set => this.SetProperty(ref this.isBay1PositionDownPresent, value); }

        public bool IsBay1PositionUpPresent { get => this.isBay1PositionUpPresent; private set => this.SetProperty(ref this.isBay1PositionUpPresent, value); }

        public bool IsBay2ExternalDoublePresent { get => this.isBay2ExternalDoublePresent; private set => this.SetProperty(ref this.isBay2ExternalDoublePresent, value); }

        public bool IsBay2ExternalPresent { get => this.isBay2ExternalPresent; private set => this.SetProperty(ref this.isBay2ExternalPresent, value); }

        public bool IsBay2InternalPresent { get => this.isBay2InternalPresent; private set => this.SetProperty(ref this.isBay2InternalPresent, value); }

        public bool IsBay2PositionDownPresent { get => this.isBay2PositionDownPresent; private set => this.SetProperty(ref this.isBay2PositionDownPresent, value); }

        public bool IsBay2PositionUpPresent { get => this.isBay2PositionUpPresent; private set => this.SetProperty(ref this.isBay2PositionUpPresent, value); }

        public bool IsBay2Present { get => this.isBay2Present; private set => this.SetProperty(ref this.isBay2Present, value); }

        public bool IsBay3ExternalDoublePresent { get => this.isBay3ExternalDoublePresent; private set => this.SetProperty(ref this.isBay3ExternalDoublePresent, value); }

        public bool IsBay3ExternalPresent { get => this.isBay3ExternalPresent; private set => this.SetProperty(ref this.isBay3ExternalPresent, value); }

        public bool IsBay3InternalPresent { get => this.isBay3InternalPresent; private set => this.SetProperty(ref this.isBay3InternalPresent, value); }

        public bool IsBay3PositionDownPresent { get => this.isBay3PositionDownPresent; private set => this.SetProperty(ref this.isBay3PositionDownPresent, value); }

        public bool IsBay3PositionUpPresent { get => this.isBay3PositionUpPresent; private set => this.SetProperty(ref this.isBay3PositionUpPresent, value); }

        public bool IsBay3Present { get => this.isBay3Present; private set => this.SetProperty(ref this.isBay3Present, value); }

        public bool IsOneTonMachine => this.bayManager.Identity.IsOneTonMachine;

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

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
            }
            if (!(bay2 is null))
            {
                this.IsBay2ExternalPresent = bay2.IsExternal && !bay2.IsDouble;
                this.IsBay2ExternalDoublePresent = bay2.IsExternal && bay2.IsDouble;
                this.IsBay2InternalPresent = !this.IsBay2ExternalPresent && !this.IsBay2ExternalDoublePresent;
            }
            if (!(bay3 is null))
            {
                this.IsBay3ExternalPresent = bay3.IsExternal && !bay3.IsDouble;
                this.IsBay3ExternalDoublePresent = bay3.IsExternal && bay3.IsDouble;
                this.IsBay3InternalPresent = !this.IsBay3ExternalPresent && !this.IsBay3ExternalDoublePresent;
            }

            this.IsBay2PositionDownPresent = (bay2?.IsDouble ?? false) || (!bay2?.Positions?.Any(o => o.IsUpper) ?? false);
            this.IsBay2PositionUpPresent = (bay2?.IsDouble ?? false) || (bay2?.Positions?.Any(o => o.IsUpper) ?? false);

            this.IsBay3PositionDownPresent = (bay3?.IsDouble ?? false) || (!bay3?.Positions?.Any(o => o.IsUpper) ?? false);
            this.IsBay3PositionUpPresent = (bay3?.IsDouble ?? false) || (bay3?.Positions?.Any(o => o.IsUpper) ?? false);

            this.sensors.Update(sensorsStates.ToArray());
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
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            this.sensors.Update(message.Data.SensorsStates);
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
                        m => m.Data?.SensorsStates != null);
        }

        #endregion
    }
}
