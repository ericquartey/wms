using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaseSensorsViewModel : BaseMainViewModel, IBaseSensorsViewModel
    {
        #region Fields

        private readonly IMachineSensorsService machineSensorsService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly Sensors sensors = new Sensors();

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseSensorsViewModel(IMachineSensorsService machineSensorsService)
            : base(Services.PresentationMode.Installer)
        {
            if (machineSensorsService == null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            this.machineSensorsService = machineSensorsService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public IEnumerable<NavigationMenuItem> MenuItems => this.menuItems;

        public Sensors Sensors => this.sensors;

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.sensors.Update(message?.Data?.SensorsStates),
                    ThreadOption.PublisherThread,
                    false);

            try
            {
                await this.machineSensorsService.ForceNotificationAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex.Message);
            }

            await base.OnNavigatedAsync();
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
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
                    Utils.Modules.Installation.Sensors.CRADLE,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Cradle,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.SHUTTER,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Shutter,
                    trackCurrentView: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.OTHERS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Others,
                    trackCurrentView: false));
        }

        #endregion
    }
}
