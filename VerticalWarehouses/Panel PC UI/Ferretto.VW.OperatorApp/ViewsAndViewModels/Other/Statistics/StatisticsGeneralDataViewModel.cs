using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BaseViewModel, IStatisticsGeneralDataViewModel
    {
        #region Fields

        private MachineStatistics model;

        readonly IMachineStatisticsService machineStatisticsService;

        public MachineStatistics Model
        {
            get => this.model;
            set => this.SetProperty(ref this.model, value);
        }

        #endregion

        #region Constructors

        public StatisticsGeneralDataViewModel(
            IStatisticsNavigationViewModel statisticsNavigationViewModel,
            IMachineStatisticsService machineStatisticsService)
        {
            if (machineStatisticsService == null)
            {
                throw new System.ArgumentNullException(nameof(machineStatisticsService));
            }

            if (statisticsNavigationViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(statisticsNavigationViewModel));
            }

            this.NavigationViewModel = statisticsNavigationViewModel as BindableBase;
            this.machineStatisticsService = machineStatisticsService;
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
            catch
            {
                //TODO call toolbar notification service
            }
        }

        #endregion
    }
}
