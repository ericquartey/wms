using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class GeneralViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly WMS.Data.WebAPI.Contracts.IDataHubClient dataHubClient;

        private readonly IMachineIdentityWebService identityService;

        private Brush machineServiceStatusBrush;

        private MachineIdentity model;

        private DelegateCommand viewStatusSensorsCommand;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

        #region Constructors

        public GeneralViewModel(
            IMachineIdentityWebService identityService,
            WMS.Data.WebAPI.Contracts.IDataHubClient dataHubClient)
            : base()
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            this.dataHubClient = dataHubClient ?? throw new ArgumentNullException(nameof(dataHubClient));
            this.dataHubClient.ConnectionStatusChanged += this.OperatorHubClient_ConnectionStatusChanged;

            this.UpdateWmsServicesStatus();
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

        public override void Disappear()
        {
            base.Disappear();
        }

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

        private void OperatorHubClient_ConnectionStatusChanged(
            object sender,
            WMS.Data.WebAPI.Contracts.ConnectionStatusChangedEventArgs e)
        {
            this.UpdateWmsServicesStatus();
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

        private void UpdateWmsServicesStatus()
        {
            if (this.dataHubClient.IsConnected)
            {
                this.WmsServicesStatusDescription = "Online"; // VW.App.Resources.OperatorApp.WmsServicesOnline;
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = "Offline"; //VW.App.Resources.OperatorApp.WmsServicesOffline;
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        #endregion
    }
}
