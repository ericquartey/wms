using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsFooterViewModel : BindableBase, IStatisticsFooterViewModel
    {
        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
