using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;

namespace Ferretto.Common.Controls.Services
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IRegionManager regionManager;
        private readonly IUnityContainer container;
        private readonly Dictionary<string, ViewModelBind> registrations = new Dictionary<string, ViewModelBind>();

        #endregion

        #region Ctor

        public NavigationService(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.container = unityContainer;
            this.regionManager = regionManager;
        }

        #endregion

        #region Public methods

        #region Appear

        public void Appear<TViewModel>()
        {
            var (moduleName, viewModelName) = MvvmNaming.GetViewModelNames<TViewModel>();
            this.Appear(moduleName, viewModelName);
        }

        public void Appear(string moduleName, string viewModelName)
        {
            if (MvvmNaming.IsViewModelNameValid(viewModelName) == false)
            {
                return;
            }

            this.LoadModule(moduleName);

            var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
            var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);

            var instanceModuleViewName = this.CheckAddRegion(moduleViewName);

            var region = this.regionManager.Regions[instanceModuleViewName];
            var view = region.Views.FirstOrDefault(v =>
                v.GetType().ToString().Equals(moduleViewName, StringComparison.InvariantCulture));

            region.Activate(view);
        }

        private string CheckAddRegion(string moduleViewName)
        {
            var viewModelBind = this.GetViewModelBind(moduleViewName);
            var instanceModuleViewName = $"{moduleViewName}.{viewModelBind.Ids.First()}";
            if (this.regionManager.Regions.ContainsRegionWithName(instanceModuleViewName) == false)
            {
                // Map Prism region to current layout
                this.AddToRegion(instanceModuleViewName);
                return instanceModuleViewName;
            }

            var idStateNotChanged = GetStateNotChanged(moduleViewName, viewModelBind);
            if (idStateNotChanged != null)
            {
                // View state is not changed, activate this id
                instanceModuleViewName = $"{moduleViewName}.{idStateNotChanged}";
            }
            else
            {
                // View state is changed, register new instance of same view type
                var newRegId = viewModelBind.GetNewId();
                instanceModuleViewName = $"{moduleViewName}.{newRegId}";
                this.container.RegisterType(typeof(INavigableViewModel), viewModelBind.ViewModel,
                    instanceModuleViewName);
                this.container.RegisterType(typeof(INavigableView), viewModelBind.View, instanceModuleViewName);

                // Map cloned type to current layout
                this.AddToRegion(instanceModuleViewName);
            }

            return instanceModuleViewName;
        }

        #endregion

        #region Disappear

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4144:Methods should not have identical implementations",
            Justification = "Method is not yet fully implemented")]
        public void Disappear<TViewModel>()
        {
            var (moduleName, viewModelName) = MvvmNaming.GetViewModelNames<TViewModel>();
            this.Disappear(moduleName, viewModelName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "CA1804",
            Justification = "Method is not yet fully implemented")]
        public void Disappear(string moduleName, string viewModelName)
        {
            if (MvvmNaming.IsViewModelNameValid(viewModelName) == false)
            {
                return;
            }

            var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
            var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);

            throw new NotSupportedException("Disappear need to be implemented");

/*
      // Get corrent mapid
      var moduleRegionName = $"{moduleName}.{modelName}.1";
      if (this.regionManager.Regions.ContainsRegionWithName(moduleRegionName) == false)
      {
        return;
      }

      var region = this.regionManager.Regions[moduleRegionName];
      var viewToRemove = region.GetView(moduleViewName);
      if (viewToRemove != null)
      {
        region.Remove(viewToRemove);
      }
*/
        }

        #endregion

        public void Register<TItemsView, TItemsViewModel>() where TItemsViewModel : INavigableViewModel
            where TItemsView : INavigableView
        {
            var newRegId = this.GetNewRegistrationId<TItemsView, TItemsViewModel>();
            this.container.RegisterType<INavigableViewModel, TItemsViewModel>(newRegId);
            this.container.RegisterType<INavigableView, TItemsView>(newRegId);
        }

        /// <summary>
        /// Registers a new view model instance for the specified view.
        /// </summary>
        /// <param name="viewName">The type name of the view associated to the view model.</param>
        /// <param name="token"></param>
        /// <remarks>This method shall be called only after the Register method has been invoked to ensure there is an association registered between the view type and the view model type.
        /// Multiple calls to this method are allowed and they will generate new instances of the view model.</remarks>
        /// <returns>A new instance of the view model associated to the specified view.</returns>
        public INavigableViewModel RegisterAndGetViewModel(string viewName, string token)
        {
            if (this.registrations.ContainsKey(viewName) == false)
            {
                throw new InvalidOperationException(
                    $"Before invoking the ({nameof(this.RegisterAndGetViewModel)}) method, the {nameof(this.Register)} method needs to be called in order to register an association between the specified view and a view model type.");
            }

            var viewModelBind = this.registrations[viewName];
            // Generate random mapId
            var mapId = Guid.NewGuid().ToString("N");
            this.container.RegisterType(typeof(INavigableViewModel), viewModelBind.ViewModel, mapId);
            var vm = ServiceLocator.Current.GetInstance<INavigableViewModel>(mapId);
            vm.Token = token;
            return vm;
        }

        public INavigableViewModel GetViewModelByName(string viewModelName)
        {
            if (MvvmNaming.IsViewModelNameValid(viewModelName) == false)
            {
                return null;
            }

            var names = MvvmNaming.GetViewModelNames(viewModelName);
            return ServiceLocator.Current.GetInstance<INavigableViewModel>(names.viewModelName);
        }

        public INavigableViewModel GetViewModelByMapId(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                throw new ArgumentException("The argument cannot be null or empty.", nameof(mapId));
            }

            return ServiceLocator.Current.GetInstance<INavigableViewModel>(mapId);
        }

        #endregion

        #region Private methods

        private string GetNewRegistrationId<TItemsView, TItemsViewModel>()
            where TItemsView : INavigableView
            where TItemsViewModel : INavigableViewModel
        {
            string newId = null;
            ViewModelBind viewModelBind = null;
            var fullViewName = typeof(TItemsView).ToString();
            if (this.registrations.ContainsKey(fullViewName) == false)
            {
                viewModelBind = new ViewModelBind(typeof(TItemsView), typeof(TItemsViewModel));
                this.registrations.Add(fullViewName, viewModelBind);
            }
            else
            {
                viewModelBind = this.registrations[fullViewName];
            }

            newId = $"{typeof(TItemsView)}.{viewModelBind.GetNewId()}";
            return newId;
        }

        private ViewModelBind GetViewModelBind(string fullViewName)
        {
            return this.registrations.ContainsKey(fullViewName) ? this.registrations[fullViewName] : null;
        }

        private static string GetStateNotChanged(string moduleViewName, ViewModelBind viewModelBind)
        {
            foreach (var id in viewModelBind.Ids)
            {
                var viewModel = ServiceLocator.Current.GetInstance<INavigableViewModel>($"{moduleViewName}.{id}");
                if (string.IsNullOrEmpty(viewModel.StateId))
                {
                    return id;
                }
            }

            return null;
        }

        private void AddToRegion(string moduleViewName)
        {
            var registeredView = ServiceLocator.Current.GetInstance<INavigableView>(moduleViewName);
            registeredView.Token = moduleViewName;
            registeredView.MapId = moduleViewName;
            WmsMainDockLayoutManager.Current.RegisterView(moduleViewName, registeredView.Title);
            this.regionManager.AddToRegion(moduleViewName, registeredView);
        }

        private void LoadModule(string moduleName)
        {
            var catalog = this.container.Resolve<IModuleCatalog>();
            var module = ( catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName) );
            if (module.State != ModuleState.NotStarted)
            {
                return;
            }

            var moduleManager = this.container.Resolve<IModuleManager>();
            moduleManager.LoadModule(moduleName);
        }

        #endregion

        #region Helper class

        private class ViewModelBind
        {
            #region Properties

            public Type View { get; set; }
            public Type ViewModel { get; set; }

            public List<string> Ids { get; set; }

            #endregion

            #region Ctor

            public ViewModelBind(Type view, Type viewModel)
            {
                this.View = view;
                this.ViewModel = viewModel;
                this.Ids = new List<string>();
            }

            #endregion

            public string GetNewId()
            {
                var newId = ( this.Ids.Count + 1 ).ToString();
                this.Ids.Add(newId);
                return newId;
            }
        }

        #endregion
    }
}
