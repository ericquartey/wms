using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.OperatorApp.Interfaces;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class MachineStatisticsViewModel : BaseViewModel, IMachineStatisticsViewModel
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

        public MachineStatisticsViewModel(IMachineStatisticsService machineStatisticsService)
        {
            if (machineStatisticsService == null)
            {
                throw new ArgumentNullException(nameof(machineStatisticsService));
            }

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
