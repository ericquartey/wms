using System;
using System.Threading.Tasks;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class OthersNavigationViewModel : BaseOthersViewModel
    {
        #region Fields

        private readonly IMachineIdentityWebService identityService;

        private Brush machineServiceStatusBrush;

        private MachineIdentity model;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

        #region Constructors

        public OthersNavigationViewModel(IMachineIdentityWebService identityService)
            : base()
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
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
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.Model = await this.identityService.GetAsync();
            this.MachineServiceStatusBrush = this.GetBrushForServiceStatus(this.Model.ServiceStatus);

            this.UpdateWmsServicesStatus(this.HealthProbeService.HealthWmsStatus);
        }

        protected override Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            this.UpdateWmsServicesStatus(e.HealthWmsStatus);

            return base.OnHealthStatusChangedAsync(e);
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

        private void UpdateWmsServicesStatus(HealthStatus wmsHealthStatus)
        {
            if (wmsHealthStatus is HealthStatus.Healthy || wmsHealthStatus is HealthStatus.Degraded)
            {
                this.WmsServicesStatusDescription = Resources.OperatorApp.WmsServicesOnline;
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = Resources.OperatorApp.WmsServicesOffline;
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        #endregion
    }
}
