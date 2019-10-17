using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaseSensorsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineSensorsWebService machineSensorsWebService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly Sensors sensors = new Sensors();

        private bool bay1HasShutter;

        private bool bay2HasShutter;

        private bool bay3HasShutter;

        private bool isBay2Present;

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

        public bool Bay2HasShutter { get => this.bay2HasShutter; private set => this.SetProperty(ref this.bay2HasShutter, value); }

        public bool Bay3HasShutter { get => this.bay3HasShutter; private set => this.SetProperty(ref this.bay3HasShutter, value); }

        public override EnableMask EnableMask => EnableMask.None;

        public bool IsBay2Present { get => this.isBay2Present; private set => this.SetProperty(ref this.isBay2Present, value); }

        public bool IsBay3Present { get => this.isBay3Present; private set => this.SetProperty(ref this.isBay3Present, value); }

        public bool IsOneTonMachine => this.bayManager.Identity.IsOneTonMachine;

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    this.OnSensorsChanged,
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsWebService.GetAsync();

                var bays = await this.machineBaysWebService.GetAllAsync();

                this.IsBay2Present = bays.Any(b => b.Number == BayNumber.BayTwo);
                this.IsBay3Present = bays.Any(b => b.Number == BayNumber.BayThree);

                this.Bay1HasShutter = bays
                    .Where(b => b.Number == BayNumber.BayOne)
                    .Select(b => b.Shutter != null)
                    .SingleOrDefault();

                this.Bay2HasShutter = bays
                    .Where(b => b.Number == BayNumber.BayTwo)
                    .Select(b => b.Shutter != null)
                    .SingleOrDefault();

                this.Bay3HasShutter = bays
                    .Where(b => b.Number == BayNumber.BayThree)
                    .Select(b => b.Shutter != null)
                    .SingleOrDefault();

                this.sensors.Update(sensorsStates.ToArray());
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void InitializeNavigationMenu()
        {
            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.VERTICALAXIS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.VerticalAxisButton,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.BAYS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Bays,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.OTHERS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Others,
                    trackCurrentView: false));
        }

        private void OnSensorsChanged(NotificationMessageUI<SensorsChangedMessageData> message)
        {
            if (message is null)
            {
                throw new System.ArgumentNullException(nameof(message));
            }

            this.sensors.Update(message.Data?.SensorsStates);
        }

        #endregion
    }
}
