using Ferretto.VW.OperatorApp.Interfaces;
using System.Threading.Tasks;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class MainWindowNavigationButtonsViewModel : BindableBase, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
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
