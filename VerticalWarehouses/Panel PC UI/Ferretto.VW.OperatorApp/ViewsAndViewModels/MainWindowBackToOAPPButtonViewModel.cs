namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    using System.Threading.Tasks;
    using Ferretto.VW.OperatorApp.Interfaces;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Unity;

    public class MainWindowBackToOAPPButtonViewModel : BindableBase, IMainWindowBackToOAPPButtonViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private string note;

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

        public string Note { get => this.note; set => this.SetProperty(ref this.note, value); }

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
