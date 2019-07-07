using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class FooterViewModel : BindableBase, IFooterViewModel
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private ICommand navigateBackCommand;

        private string note;

        #endregion

        #region Constructors

        public FooterViewModel(
            IEventAggregator eventAggregator,
            IUnityContainer container)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand NavigateBackCommand =>
            this.navigateBackCommand
            ??
            (this.navigateBackCommand = new DelegateCommand(
                () => this.container.Resolve<INavigationService>().NavigateFromView()));

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
            this.navigateBackCommand = null;
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
