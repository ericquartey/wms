using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.App.Installation.Resources;
using Ferretto.VW.App.Installation.Resources.Enumerables;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels
{
    public class FooterViewModel : BindableBase, IFooterViewModel
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly IStatusMessageService statusMessageService;

        private Visibility cancelButtonVisibility = Visibility.Hidden;

        private bool isBackButtonActive = true;

        private bool isCancelButtonActive;

        private ICommand navigateBackCommand;

        private string note;

        #endregion

        #region Constructors

        public FooterViewModel(
            IEventAggregator eventAggregator,
            IStatusMessageService statusMessageService,
            IUnityContainer container)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (statusMessageService == null)
            {
                throw new System.ArgumentNullException(nameof(statusMessageService));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
            this.statusMessageService = statusMessageService;
            this.statusMessageService.StatusMessageNotified += this.StatusMessageService_StatusMessageNotified;
        }

        #endregion

        #region Properties

        public CompositeCommand CancelButtonCommand { get; set; }

        public Visibility CancelButtonVisibility { get => this.cancelButtonVisibility; set => this.SetProperty(ref this.cancelButtonVisibility, value); }

        public bool IsBackButtonActive { get => this.isBackButtonActive; set => this.SetProperty(ref this.isBackButtonActive, value); }

        public bool IsCancelButtonActive { get => this.isCancelButtonActive; set => this.SetProperty(ref this.isCancelButtonActive, value); }

        public ICommand NavigateBackCommand =>
            this.navigateBackCommand
            ??
            (this.navigateBackCommand = new DelegateCommand(() => this.NavigateBack()));

        public BindableBase NavigationViewModel { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string Note { get => this.note; set => this.SetProperty(ref this.note, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            return null;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private void NavigateBack()
        {
            var mainWindowViewModel = (MainWindowViewModel)this.container.Resolve<IMainWindowViewModel>();

            (mainWindowViewModel.NavigationRegionCurrentViewModel as IViewModel)?.ExitFromViewMethod();

            mainWindowViewModel.NavigationRegionCurrentViewModel = (MainWindowNavigationButtonsViewModel)this.container.Resolve<IMainWindowNavigationButtonsViewModel>();
            mainWindowViewModel.ContentRegionCurrentViewModel = this.container.Resolve<IIdleViewModel>();

            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(
                new InstallationApp_EventMessage(InstallationApp_EventMessageType.ExitView));
        }

        private void StatusMessageService_StatusMessageNotified(object sender, StatusMessageEventArgs e)
        {
            this.Note = e.Message;
        }

        #endregion
    }
}
