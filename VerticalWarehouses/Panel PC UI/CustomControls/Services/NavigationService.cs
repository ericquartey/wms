using System;
using System.Linq;
using System.Windows;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.Utils;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.VW.App.Services
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IRegionManager regionManager;

        #endregion

        #region Constructors

        public NavigationService(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.container = unityContainer;
            this.regionManager = regionManager;
        }

        #endregion

        #region Methods

        public void Appear(string moduleName, string viewModelName, object data = null)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                return;
            }

            this.logger.Trace(string.Format("Opening view '{0}' of module '{1}'.", viewModelName, moduleName));

            try
            {
                this.LoadModule(moduleName);

                var viewName = MvvmNaming.GetViewNameFromViewModelName(viewModelName);

                var parameters = new NavigationParameters();
                parameters.Add(viewModelName, data);

                this.regionManager.RequestNavigate(Ferretto.VW.Utils.Modules.Layout.REGION_MAINCONTENT, viewName, parameters);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Cannot show view '{0}' for module '{1}'.", viewModelName, moduleName));
            }

            return;
        }

        public void Disappear(INavigableViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            try
            {
                viewModel.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Cannot close view model '{0}'.", viewModel.GetType().Name));
            }
        }

        public void LoadModule(string moduleName)
        {
            var catalog = this.container.Resolve<IModuleCatalog>();
            var module = catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName);
            if (module == null)
            {
                this.logger.Error($"Module {moduleName} not found.");
                return;
            }

            if (module.State != ModuleState.NotStarted)
            {
                return;
            }

            var moduleManager = this.container.Resolve<IModuleManager>();
            moduleManager.LoadModule(moduleName);
        }

        public void SetBusy(bool isBusy)
        {
            if (Application.Current.MainWindow.Descendants<View>().FirstOrDefault() is View view &&
                view.DataContext is INavigableViewModel layoutViewModel)
            {
                layoutViewModel.IsBusy = isBusy;
            }
        }

        #endregion
    }
}
