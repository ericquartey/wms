using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class GeneralViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private Brush machineServiceStatusBrush;

        private MachineIdentity model;

        private DelegateCommand viewStatusSensorsCommand;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

        #region Constructors

        public GeneralViewModel(IMachineIdentityWebService identityService)
            : base()
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

            this.UpdateWmsServicesStatus(this.HealthProbeService.HealthWmsStatus);
        }

        #endregion

        #region Properties

        public Brush MachineServiceStatusBrush
        {
            get => this.machineServiceStatusBrush;
            set => this.SetProperty(ref this.machineServiceStatusBrush, value);
        }

        public MachineIdentity Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        public string SoftwareVersion => this.GetType().Assembly.GetName().Version.ToString();

        public ICommand ViewStatusSensorsCommand =>
            this.viewStatusSensorsCommand
            ??
            (this.viewStatusSensorsCommand = new DelegateCommand(
                () => this.StatusSensorsCommand(),
                this.CanExecuteCommand));

        public Brush WmsServicesStatusBrush
        {
            get => this.wmsServicesStatusBrush;
            set => this.SetProperty(ref this.wmsServicesStatusBrush, value);
        }

        public string WmsServicesStatusDescription
        {
            get => this.wmsServicesStatusDescription;
            set => this.SetProperty(ref this.wmsServicesStatusDescription, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        internal bool CanExecuteCommand()
        {
            return true;
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.Model = await this.identityService.GetAsync();

            this.MachineServiceStatusBrush = this.GetBrushForServiceStatus(this.Model.ServiceStatus);
        }

        protected override Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            this.UpdateWmsServicesStatus(e.HealthWmsStatus);

            return base.OnHealthStatusChangedAsync(e);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.viewStatusSensorsCommand?.RaiseCanExecuteChanged();
        }

        private Brush GetBrushForServiceStatus(MachineServiceStatus serviceStatus)
        {
            switch (serviceStatus)
            {
                case MachineServiceStatus.Expired:
                    return Brushes.Red;

                case MachineServiceStatus.Expiring:
                    return Brushes.Gold;

                case MachineServiceStatus.Valid:
                    return Brushes.Green;

                default:
                    return Brushes.Gray;
            }
        }

        private void StatusSensorsCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Sensors.SECURITY,
                    data: null,
                    trackCurrentView: true);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void UpdateWmsServicesStatus(HealthStatus wmsHealthStatus)
        {
            if (wmsHealthStatus is HealthStatus.Healthy || wmsHealthStatus is HealthStatus.Degraded)
            {
                this.WmsServicesStatusDescription = "Online";
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = "Offline";
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        #endregion
    }
}
