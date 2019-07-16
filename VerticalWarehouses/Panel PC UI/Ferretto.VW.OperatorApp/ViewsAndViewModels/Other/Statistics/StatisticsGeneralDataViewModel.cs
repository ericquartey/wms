using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BaseViewModel, IStatisticsGeneralDataViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public StatisticsGeneralDataViewModel(
            IEventAggregator eventAggregator,
            IStatisticsNavigationViewModel statisticsNavigationViewModel)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = statisticsNavigationViewModel as BindableBase;
        }

        #endregion

    }
}
