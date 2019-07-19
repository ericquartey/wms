using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BaseViewModel, IStatisticsGeneralDataViewModel
    {
        #region Fields

        private readonly IMachineStatisticsService machineStatisticsService;

        private readonly IStatusMessageService statusMessageService;

        private MachineStatistics model;

        #endregion

        #region Constructors

        public StatisticsGeneralDataViewModel(
            IStatisticsNavigationViewModel statisticsNavigationViewModel,
            IMachineStatisticsService machineStatisticsService,
            IStatusMessageService statusMessageService)
        {
            if (machineStatisticsService == null)
            {
                throw new System.ArgumentNullException(nameof(machineStatisticsService));
            }

            if (statusMessageService == null)
            {
                throw new System.ArgumentNullException(nameof(statusMessageService));
            }

            if (statisticsNavigationViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(statisticsNavigationViewModel));
            }

            this.NavigationViewModel = statisticsNavigationViewModel as BindableBase;
            this.machineStatisticsService = machineStatisticsService;
            this.statusMessageService = statusMessageService;
        }

        #endregion

        #region Properties

        public MachineStatistics Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            try
            {
                this.Model = await this.machineStatisticsService.GetAsync();

                await base.OnEnterViewAsync();
            }
            catch (System.Exception ex)
            {
                this.statusMessageService.Notify(ex);
            }
        }

        #endregion
    }
}
