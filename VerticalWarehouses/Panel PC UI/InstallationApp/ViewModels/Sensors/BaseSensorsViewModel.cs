using System.ComponentModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaseSensorsViewModel : BaseMainViewModel, IBaseSensorsViewModel
    {
        #region Fields

        private readonly ISensorsMachineService sensorsMachineService;

        private BindingList<NavigationMenuItem> menuItems = new BindingList<NavigationMenuItem>();

        private bool[] sensorsStates;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        protected BaseSensorsViewModel(ISensorsMachineService sensorsMachineService)
            : base(Services.PresentationMode.Installator)
        {
            if (sensorsMachineService == null)
            {
                throw new System.ArgumentNullException(nameof(sensorsMachineService));
            }

            this.sensorsMachineService = sensorsMachineService;

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.VERTICALAXIS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.VerticalAxisButton,
                    canBackTrack: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.BAYS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Bays,
                    canBackTrack: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.CRADLE,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Cradle,
                    canBackTrack: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.SHUTTER,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Shutter,
                    canBackTrack: false));

            this.menuItems.Add(
                new NavigationMenuItem(
                    Utils.Modules.Installation.Sensors.OTHERS,
                    nameof(Utils.Modules.Installation),
                    VW.App.Resources.InstallationApp.Others,
                    canBackTrack: false));
        }

        #endregion

        #region Properties

        public BindingList<NavigationMenuItem> MenuItems
        {
            get => this.menuItems;
            set => this.SetProperty(ref this.menuItems, value);
        }

        public bool[] SensorsStates
        {
            get => this.sensorsStates;
            set => this.SetProperty(ref this.sensorsStates, value);
        }

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            this.subscriptionToken = this.EventAggregator
                .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message =>
                        this.SensorsStates = message?.Data?.SensorsStates,
                    ThreadOption.PublisherThread,
                    false);

            try
            {
                await this.sensorsMachineService.ForceNotificationAsync();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex.Message);
            }

            await base.OnNavigatedAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.subscriptionToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
