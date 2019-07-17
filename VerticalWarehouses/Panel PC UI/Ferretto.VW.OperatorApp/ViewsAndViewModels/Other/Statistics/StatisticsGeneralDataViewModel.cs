using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsGeneralDataViewModel : BaseViewModel, IStatisticsGeneralDataViewModel
    {

        #region Constructors

        public StatisticsGeneralDataViewModel(
            IStatisticsNavigationViewModel statisticsNavigationViewModel)
        {
            if (statisticsNavigationViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(statisticsNavigationViewModel));
            }

            this.NavigationViewModel = statisticsNavigationViewModel as BindableBase;
        }

        #endregion
    }
}
