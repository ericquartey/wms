using System.Threading.Tasks;
using System.Windows.Media;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class GeneralInfoViewModel : BaseViewModel, IGeneralInfoViewModel
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IDataHubClient dataHubClient;

        private readonly IIdentityService identityService;

        private Brush machineServiceStatusBrush;

        private MachineIdentity model;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

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

        public IOtherNavigationViewModel OtherNavigationViewModel { get; }

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

        private void OperatorHubClient_ConnectionStatusChanged(
            object sender,
            WMS.Data.WebAPI.Contracts.ConnectionStatusChangedEventArgs e)
        {
            this.UpdateWmsServicesStatus();
        }

        private void UpdateWmsServicesStatus()
        {
            if (this.dataHubClient.IsConnected)
            {
                this.WmsServicesStatusDescription = App.Resources.OperatorApp.WmsServicesOnline;
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = App.Resources.OperatorApp.WmsServicesOffline;
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        #endregion
    }
}
