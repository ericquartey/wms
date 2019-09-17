using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaseSensorsViewModel : BaseMainViewModel, IBaseSensorsViewModel
    {
        #region Fields

        private readonly MAS.AutomationService.Contracts.IMachineSensorsService machineSensorsService;

        private readonly BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private readonly Sensors sensors = new Sensors();

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseSensorsViewModel(
            IMachineSensorsService machineSensorsService)
            : base(Services.PresentationMode.Installer)
        {
            if (machineSensorsService is null)
            {
                throw new System.ArgumentNullException(nameof(machineSensorsService));
            }

            this.machineSensorsService = machineSensorsService;

            this.InitializeNavigationMenu();
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.None;

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

        public override async Task OnNavigatedAsync()
        {
            await base.OnNavigatedAsync();

            this.IsBackNavigationAllowed = true;

            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.sensors.Update(message?.Data?.SensorsStates),
                    ThreadOption.UIThread,
                    false);

            try
            {
                var sensorsStates = await this.machineSensorsService.GetAsync();

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

        #endregion
    }
}
