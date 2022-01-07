using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class GeneralViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineIdentityWebService identityService;

        private readonly IMachineAboutWebService machineAboutWebService;

        private int averageHeight;

        private int averageOccupiedSpace;

        private Brush machineServiceStatusBrush;

        private MachineIdentity model;

        private MachineStatistics statistics;

        private int totalDrawersCounter;

        private double totalDrawersWeight;

        private int totalMissionCounter;

        private DelegateCommand viewStatusSensorsCommand;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

        #region Constructors

        public GeneralViewModel(
            IMachineIdentityWebService identityService,
            IMachineAboutWebService machineAboutWebService,
            IHealthProbeService healthProbeService)
            : base()
        {
            this.identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));

            this.machineAboutWebService = machineAboutWebService ?? throw new ArgumentNullException(nameof(machineAboutWebService));

            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));

            this.UpdateWmsServicesStatus(this.HealthProbeService.HealthWmsStatus);
        }

        #endregion

        #region Properties

        public int AverageHeight
        {
            get => this.averageHeight;
            set => this.SetProperty(ref this.averageHeight, value);
        }

        public int AverageOccupiedSpace
        {
            get => this.averageOccupiedSpace;
            set => this.SetProperty(ref this.averageOccupiedSpace, value);
        }

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

        public string SoftwareVersion => System.Reflection.Assembly
            .GetEntryAssembly()
            .GetName()
            .Version
            .ToString();

        public MachineStatistics Statistics
        {
            get => this.statistics;
            set => this.SetProperty(ref this.statistics, value);
        }

        public int TotalDrawersCounter
        {
            get => this.totalDrawersCounter;
            set => this.SetProperty(ref this.totalDrawersCounter, value);
        }

        public double TotalDrawersWeight
        {
            get => this.totalDrawersWeight;
            set => this.SetProperty(ref this.totalDrawersWeight, value);
        }

        public int TotalMissionCounter
        {
            get => this.totalMissionCounter;
            set => this.SetProperty(ref this.totalMissionCounter, value);
        }

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
            try
            {
                await base.OnDataRefreshAsync();

                this.Model = await this.identityService.GetAsync();

                this.Statistics = await this.identityService.GetStatisticsAsync();

                this.TotalMissionCounter = await this.machineAboutWebService.MissionTotalNumberAsync();
                this.TotalDrawersWeight = this.MachineService.Loadunits.Sum(s => s.GrossWeight);
                this.TotalDrawersCounter = this.MachineService.Loadunits.Count();
                var loadUnits = this.MachineService.Loadunits.Count(lu => lu.IsIntoMachineOrBlocked);
                this.AverageOccupiedSpace = loadUnits > 0 ?
                    (int)Math.Round(this.MachineService.Cells.Count(c => !c.IsFree) * 25.0 / loadUnits)
                    : 0;
                this.AverageHeight = this.AverageOccupiedSpace > 30 ? this.AverageOccupiedSpace - 30 : 0;

                this.MachineServiceStatusBrush = this.GetBrushForServiceStatus(this.healthProbeService.HealthMasStatus);

                this.UpdateWmsServicesStatus(this.healthProbeService.HealthWmsStatus);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override Task OnHealthStatusChangedAsync(HealthStatusChangedEventArgs e)
        {
            this.MachineServiceStatusBrush = this.GetBrushForServiceStatus(e.HealthMasStatus);

            this.UpdateWmsServicesStatus(e.HealthWmsStatus);

            return base.OnHealthStatusChangedAsync(e);
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await this.OnDataRefreshAsync();

            await base.OnMachineStatusChangedAsync(e);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.viewStatusSensorsCommand?.RaiseCanExecuteChanged();
        }

        private Brush GetBrushForServiceStatus(HealthStatus serviceStatus)
        {
            switch (serviceStatus)
            {
                case HealthStatus.Initialized:
                case HealthStatus.Initializing:
                    return Brushes.Gold;

                case HealthStatus.Healthy:
                    return Brushes.Green;

                default:
                    return Brushes.Red;
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
                this.WmsServicesStatusDescription = Resources.Localized.Get("OperatorApp.WmsServicesOnline");
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else
            {
                this.WmsServicesStatusDescription = Resources.Localized.Get("OperatorApp.WmsServicesOffline");
                this.WmsServicesStatusBrush = Brushes.Red;
            }
        }

        #endregion
    }
}
