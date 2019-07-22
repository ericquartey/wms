namespace Ferretto.VW.InstallationApp
{
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Ferretto.VW.InstallationApp.Resources;
    using Ferretto.VW.InstallationApp.Resources.Enumerables;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Unity;

    public class SSNavigationButtonsViewModel : BindableBase, ISSNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand baysButtonCommand;

        private IUnityContainer container;

        private ICommand variousButtonCommand;

        private ICommand verticalButtonCommand;

        #endregion

        #region Constructors

        public SSNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand BaysButtonCommand => this.baysButtonCommand ?? (this.baysButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator
                .GetEvent<InstallationApp_Event>()
                .Subscribe(
                    async (message) => { await ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>()).OnEnterViewAsync(); },
                    ThreadOption.PublisherThread,
                    false,
                    message => message.Type == InstallationApp_EventMessageType.EnterView);

            this.eventAggregator
                .GetEvent<InstallationApp_Event>()
                .Subscribe(
                    (message) => { ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>()).UnSubscribeMethodFromEvent(); },
                    ThreadOption.PublisherThread,
                    false,
                    message => message.Type == InstallationApp_EventMessageType.ExitView);

            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>());
        }));

        public BindableBase NavigationViewModel { get; set; }

        public ICommand VariousButtonCommand => this.variousButtonCommand ?? (this.variousButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { (this.container.Resolve<ISSVariousInputsViewModel>() as SSVariousInputsViewModel)?.OnEnterViewAsync(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((SSVariousInputsViewModel)this.container.Resolve<ISSVariousInputsViewModel>()).UnSubscribeMethodFromEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSVariousInputsViewModel)this.container.Resolve<ISSVariousInputsViewModel>();
        }));

        public ICommand VerticalButtonCommand => this.verticalButtonCommand ?? (this.verticalButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                    (message) => { ((SSVerticalAxisViewModel)this.container.Resolve<ISSVerticalAxisViewModel>()).OnEnterViewAsync(); },
                    ThreadOption.PublisherThread,
                    false,
                    message => message.Type == InstallationApp_EventMessageType.EnterView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((SSVerticalAxisViewModel)this.container.Resolve<ISSVerticalAxisViewModel>()).UnSubscribeMethodFromEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSVerticalAxisViewModel)this.container.Resolve<ISSVerticalAxisViewModel>();
        }));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
