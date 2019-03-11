using System.Windows.Input;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSNavigationButtonsViewModel : BindableBase, IViewModel, ISSNavigationButtonsViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private ICommand baysButtonCommand;

        private IUnityContainer container;

        private ICommand cradleButtonCommand;

        private IEventAggregator eventAggregator;

        private ICommand gateButtonCommand;

        private ICommand variousButtonCommand;

        private ICommand verticalButtonCommand;

        #endregion

        #region Constructors

        public SSNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public ICommand BaysButtonCommand => this.baysButtonCommand ?? (this.baysButtonCommand = new DelegateCommand(() =>
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>()).SubscribeMethodToEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.EnterView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe(
                (message) => { ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>()).UnSubscribeMethodFromEvent(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.ExitView);
            this.eventAggregator.GetEvent<InstallationApp_Event>().Publish(new InstallationApp_EventMessage(InstallationApp_EventMessageType.EnterView));
            ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = ((SSBaysViewModel)this.container.Resolve<ISSBaysViewModel>());
        }));

        public ICommand CradleButtonCommand => this.cradleButtonCommand ?? (this.cradleButtonCommand = new DelegateCommand(() => ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSCradleViewModel)this.container.Resolve<ISSCradleViewModel>()));

        public ICommand GateButtonCommand => this.gateButtonCommand ?? (this.gateButtonCommand = new DelegateCommand(() => ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSShutterViewModel)this.container.Resolve<ISSShutterViewModel>()));

        public ICommand VariousButtonCommand => this.variousButtonCommand ?? (this.variousButtonCommand = new DelegateCommand(() => ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSVariousInputsViewModel)this.container.Resolve<ISSVariousInputsViewModel>()));

        public ICommand VerticalButtonCommand => this.verticalButtonCommand ?? (this.verticalButtonCommand = new DelegateCommand(() => ((SSMainViewModel)this.container.Resolve<ISSMainViewModel>()).SSContentRegionCurrentViewModel = (SSVerticalAxisViewModel)this.container.Resolve<ISSVerticalAxisViewModel>()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
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
