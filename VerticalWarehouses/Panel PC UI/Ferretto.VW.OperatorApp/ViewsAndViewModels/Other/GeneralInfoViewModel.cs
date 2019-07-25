using System.Threading.Tasks;
using System.Windows.Media;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.Other
{
    public class GeneralInfoViewModel : BaseViewModel, IGeneralInfoViewModel
    {
        private readonly IIdentityService identityService;

        readonly WMS.Data.WebAPI.Contracts.IDataHubClient dataHubClient;

        private MachineIdentity model;

        private string wmsServicesStatusDescription;

        private Brush wmsServicesStatusBrush;

        private Brush machineServiceStatusBrush;

        #region Constructors

        public GeneralInfoViewModel(
            IIdentityService identityService,
            Ferretto.WMS.Data.WebAPI.Contracts.IDataHubClient dataHubClient,
            IOtherNavigationViewModel otherNavigationViewModel)
        {
            if (identityService == null)
            {
                throw new System.ArgumentNullException(nameof(identityService));
            }

            if (otherNavigationViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(otherNavigationViewModel));
            }

            this.identityService = identityService;
            this.dataHubClient = dataHubClient;
            this.OtherNavigationViewModel = otherNavigationViewModel;

            this.dataHubClient.ConnectionStatusChanged += this.OperatorHubClient_ConnectionStatusChanged;
            this.UpdateWmsServicesStatus();

            this.NavigationViewModel = otherNavigationViewModel as OtherNavigationViewModel;
        }

        private void UpdateWmsServicesStatus()
        {
            if (true/*this.dataHubClient.IsConnected*/)
            {
                this.WmsServicesStatusDescription = VW.App.Resources.OperatorApp.WmsServicesOnline;
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = VW.App.Resources.OperatorApp.WmsServicesOffline;
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        private void OperatorHubClient_ConnectionStatusChanged(
            object sender,
            WMS.Data.WebAPI.Contracts.ConnectionStatusChangedEventArgs e)
        {
            this.UpdateWmsServicesStatus();
        }

        #endregion

        public override async Task OnEnterViewAsync()
        {
            this.Model = await this.identityService.GetAsync();
            this.MachineServiceStatusBrush = this.GetBrushForServiceStatus(this.Model.ServiceStatus);

            await base.OnEnterViewAsync();
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

        #region Properties

        public IOtherNavigationViewModel OtherNavigationViewModel { get; }

        public MachineIdentity Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        public string SoftwareVersion => this.GetType().Assembly.GetName().Version.ToString();

        public string WmsServicesStatusDescription
        {
            get => this.wmsServicesStatusDescription;
            set => this.SetProperty(ref this.wmsServicesStatusDescription, value);
        }

        public Brush WmsServicesStatusBrush
        {
            get => this.wmsServicesStatusBrush;
            set => this.SetProperty(ref this.wmsServicesStatusBrush, value);
        }

        public Brush MachineServiceStatusBrush
        {
            get => this.machineServiceStatusBrush;
            set => this.SetProperty(ref this.machineServiceStatusBrush, value);
        }

        #endregion
    }
}
