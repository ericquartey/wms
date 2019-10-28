using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BaseViewModel, IStatisticsGeneralDataViewModel
    {
        //private readonly IMachineStatisticsService statisticsService;

        //private readonly IStatusMessageService statusMessageService;

        #region Fields

        private MachineStatistics model;

        #endregion

        #region Constructors

        public StatisticsGeneralDataViewModel(
            IStatisticsNavigationViewModel statisticsNavigationViewModel/*,*/
                                                                        //IMachineStatisticsService statisticsService,
                                                                        /*IStatusMessageService statusMessageService*/)
        {
            //if (statisticsService == null)
            //{
            //    throw new System.ArgumentNullException(nameof(statisticsService));
            //}

            //if (statusMessageService == null)
            //{
            //    throw new System.ArgumentNullException(nameof(statusMessageService));
            //}

            if (statisticsNavigationViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(statisticsNavigationViewModel));
            }

            this.NavigationViewModel = statisticsNavigationViewModel as BindableBase;
            //this.statisticsService = statisticsService;
            //this.statusMessageService = statusMessageService;
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
                //this.Model = await this.statisticsService.GetAsync();

                await base.OnEnterViewAsync();
            }
            catch (System.Exception ex)
            {
                //this.statusMessageService.Notify(ex);
            }
        }

        #endregion
    }
}
