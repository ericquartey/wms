using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    internal sealed class InverterStatisticsViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IMachineServicingWebService machineServicingWebService;

        private MachineStatistics lastServiceStatistics;

        private MachineStatistics totalStatistics;

        #endregion

        #region Constructors

        public InverterStatisticsViewModel(
            IMachineServicingWebService machineServicingWebService)
            : base()
        {
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
        }

        #endregion

        #region Properties

        public MachineStatistics LastServiceStatistics
        {
            get => this.lastServiceStatistics;
            set => this.SetProperty(ref this.lastServiceStatistics, value);
        }

        public MachineStatistics TotalStatistics
        {
            get => this.totalStatistics;
            set => this.SetProperty(ref this.totalStatistics, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                await base.OnAppearedAsync();

                this.IsBackNavigationAllowed = true;
            }
            catch
            {
                // do nothing
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                await base.OnDataRefreshAsync();

                var allServicing = await this.machineServicingWebService.GetAllAsync();

                this.TotalStatistics = new MachineStatistics();

                this.TotalStatistics.InverterStatistics = allServicing.SelectMany(x => x.MachineStatistics.InverterStatistics).Reverse();
                this.TotalStatistics.TotalInverterMissionTime = allServicing.Select(s => s.MachineStatistics.TotalInverterMissionTime).Sum();
                this.TotalStatistics.TotalInverterPowerOnTime = allServicing.Select(s => s.MachineStatistics.TotalInverterPowerOnTime).Sum();

                this.RaisePropertyChanged(nameof(this.TotalStatistics));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task OnMachineStatusChangedAsync(MachineStatusChangedMessage e)
        {
            await this.OnDataRefreshAsync();

            await base.OnMachineStatusChangedAsync(e);
        }

        #endregion
    }
}
