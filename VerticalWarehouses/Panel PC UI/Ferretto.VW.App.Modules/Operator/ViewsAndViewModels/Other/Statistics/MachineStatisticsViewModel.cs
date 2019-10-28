using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other.Statistics
{
    public class MachineStatisticsViewModel : BaseViewModel, IMachineStatisticsViewModel
    {
        //private readonly IMachineStatisticsService statisticsService;

        #region Fields

        private MachineStatistics model;

        #endregion

        #region Constructors

        public MachineStatisticsViewModel(/*IMachineStatisticsService statisticsService*/)
        {
            //if (statisticsService == null)
            //{
            //    throw new ArgumentNullException(nameof(statisticsService));
            //}

            //this.statisticsService = statisticsService;
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
            catch
            {
                //TODO call toolbar notification service
            }
        }

        #endregion
    }
}
