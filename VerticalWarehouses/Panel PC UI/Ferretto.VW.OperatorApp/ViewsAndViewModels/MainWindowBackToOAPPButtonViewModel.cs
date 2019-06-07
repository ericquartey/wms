using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class MainWindowBackToOAPPButtonViewModel : BindableBase, IMainWindowBackToOAPPButtonViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        #endregion

        #region Constructors

        public MainWindowBackToOAPPButtonViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public CompositeCommand BackButtonCommand { get; set; }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void FinalizeBottomButtons()
        {
            this.BackButtonCommand = null;
        }

        public void InitializeButtons()
        {
            this.BackButtonCommand = new CompositeCommand();
            this.BackButtonCommand.RegisterCommand(new DelegateCommand(() => NavigationService.NavigateFromView()));
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
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
