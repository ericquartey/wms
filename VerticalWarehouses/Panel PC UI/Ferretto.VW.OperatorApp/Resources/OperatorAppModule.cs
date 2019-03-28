using Ferretto.VW.OperatorApp.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Events;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other;

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
            var drawerActivityVMInstance = new DrawerActivityViewModel(container.Resolve<IEventAggregator>());
            var drawerWaitVMInstance = new DrawerWaitViewModel(container.Resolve<IEventAggregator>());
            var drawerActivityDetailVMInstance = new DrawerActivityDetailViewModel(container.Resolve<IEventAggregator>());
            var listsInWaitVMInstance = new ListsInWaitViewModel(container.Resolve<IEventAggregator>());
            var itemSearchVMInstance = new ItemSearchViewModel(container.Resolve<IEventAggregator>());
            var itemDetailVMInstance = new ItemDetailViewModel(container.Resolve<IEventAggregator>());
            var immediateDrawerCallVMInstance = new ImmediateDrawerCallViewModel(container.Resolve<IEventAggregator>());
            var generalInfoVMInstance = new GeneralInfoViewModel(container.Resolve<IEventAggregator>());



            this.container.RegisterInstance<IMainWindowViewModel>(mainWindowVMInstance);
            this.container.RegisterInstance<IMainWindow>(mainWindowInstance);
            this.container.RegisterInstance<IIdleViewModel>(idleVMInstance);
            this.container.RegisterInstance<IMainWindowBackToOAPPButtonViewModel>(mainWindowBackToOAPPButtonVMInstance);
            this.container.RegisterInstance<IMainWindowNavigationButtonsViewModel>(mainWindowNavigationButtonsVMInstance);
            this.container.RegisterInstance<IHelpMainWindow>(helpMainWindowInstance);
            this.container.RegisterInstance<IDrawerActivityViewModel>(drawerActivityVMInstance);
            this.container.RegisterInstance<IDrawerWaitViewModel>(drawerWaitVMInstance);
            this.container.RegisterInstance<IDrawerActivityDetailViewModel>(drawerActivityDetailVMInstance);
            this.container.RegisterInstance<IListsInWaitViewModel>(listsInWaitVMInstance);
            this.container.RegisterInstance<IItemSearchViewModel>(itemSearchVMInstance);
            this.container.RegisterInstance<IItemDetailViewModel>(itemDetailVMInstance);
            this.container.RegisterInstance<IImmediateDrawerCallViewModel>(immediateDrawerCallVMInstance);
            this.container.RegisterInstance<IGeneralInfoViewModel>(generalInfoVMInstance);


            mainWindowVMInstance.InitializeViewModel(this.container);
            mainWindowBackToOAPPButtonVMInstance.InitializeViewModel(this.container);
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
