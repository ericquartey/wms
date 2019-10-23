using System.Configuration;
using System.Windows;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Views;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Modules.Operator
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Major Code Smell",
    "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
    Justification = "This is a container initialization class, so it is ok to be coupled to many types.")]
    [Module(ModuleName = nameof(Utils.Modules.Installation), OnDemand = true)]
    [ModuleDependency(nameof(Utils.Modules.Errors))]
    public class OperatorAppModule : IModule
    {
        #region Fields

        private readonly string automationServiceUrl = ConfigurationManager.AppSettings.Get("AutomationService:Url");

        private readonly IUnityContainer container;

        #endregion

        #region Constructors

        public OperatorAppModule(IUnityContainer container)
        {
            this.container = container;
        }

        #endregion

        #region Methods

        public static void BindViewModelToView<TViewModel, TView>(IContainerProvider containerProvider)
        {
            ViewModelLocationProvider.Register(
                typeof(TView).ToString(),
                () => containerProvider.Resolve<TViewModel>());
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.UseMachineAutomationHubs();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OperatorMenuView>();
            containerRegistry.RegisterForNavigation<EmptyView>();

            containerRegistry.RegisterForNavigation<OthersNavigationView>();
            containerRegistry.RegisterForNavigation<ImmediateDrawerCallView>();
            containerRegistry.RegisterForNavigation<DrawerCompactingView>();

            containerRegistry.RegisterForNavigation<StatisticsNavigationView>();

            containerRegistry.RegisterForNavigation<MaintenanceView>();
        }

        #endregion
    }
}
