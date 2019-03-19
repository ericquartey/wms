using Ferretto.VW.OperatorApp.ViewsAndViewModels.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Events;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;

namespace Ferretto.VW.OperatorApp.Resources
{
    public class OperatorAppModule : IModule
    {
        #region Fields

        private IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
        {
            this.container = container;

            var mainWindowVMInstance = new MainWindowViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowInstance = new MainWindow(this.container.Resolve<IEventAggregator>());
            var idleVMInstance = new IdleViewModel(container.Resolve<IEventAggregator>());
            var mainWindowBackToOAPPButtonVMInstance = new MainWindowBackToOAPPButtonViewModel(this.container.Resolve<IEventAggregator>());
            var mainWindowNavigationButtonsVMInstance = new MainWindowNavigationButtonsViewModel(this.container.Resolve<IEventAggregator>());
            var helpMainWindowInstance = new HelpMainWindow(container.Resolve<IEventAggregator>());


            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IMainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);


            mainWindowVMInstance.InitializeViewModel(this.container);
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // HACK IModule interface requires the implementation of this method
        }

        #endregion
    }
}
