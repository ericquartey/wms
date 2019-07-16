﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CommonServiceLocator;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Interfaces;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Ferretto.WMS.App.Controls.Services
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly Dictionary<string, INavigableView> dialogs = new Dictionary<string, INavigableView>();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IRegionManager regionManager;

        private readonly Dictionary<string, ViewModelBind> registrations = new Dictionary<string, ViewModelBind>();

        private INavigableView lastViewActivated;

        #endregion

        #region Constructors

        public NavigationService(IUnityContainer unityContainer, IRegionManager regionManager)
        {
            this.container = unityContainer;
            this.regionManager = regionManager;
        }

        #endregion

        #region Properties

        public bool IsUnitTest { get; set; }

        #endregion

        #region Methods

        public INavigableView GetView(string moduleViewName, object data = null)
        {
            var registeredView = ServiceLocator.Current.GetInstance<INavigableView>(moduleViewName);
            registeredView.Token = moduleViewName;
            registeredView.MapId = moduleViewName;
            registeredView.Data = data;
            return registeredView;
        }

        public void Appear<TViewModel>()
        {
            var (moduleName, viewModelName) = MvvmNaming.GetViewModelNames<TViewModel>();
            this.Appear(moduleName, viewModelName);
        }

        public INavigableView Appear(string moduleName, string viewModelName, object data = null)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                return null;
            }

            this.logger.Trace(string.Format("Opening view '{0}' of module '{1}'.", viewModelName, moduleName));

            try
            {
                this.LoadModule(moduleName);

                var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
                var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);

                return this.CheckAddRegion(moduleViewName, data);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Cannot show view '{0}' for module '{1}'.", viewModelName, moduleName));
            }

            return null;
        }

        public void Disappear(INavigableView view)
        {
            if (view == null)
            {
                return;
            }

            this.logger.Trace(string.Format("Closing view '{0}' (mapId='{1}').", view.Title, view.MapId));

            try
            {
                if (view.ViewType == WmsViewType.Docking)
                {
                    this.RemoveRegion(view.MapId);
                }
                else
                {
                    this.dialogs.Remove(view.MapId);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Cannot close view '{0}' (mapId='{1}').", view.Title, view.MapId));
            }
        }

        public void Disappear(INavigableViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            try
            {
                if (viewModel.MapId != null &&
                this.dialogs.ContainsKey(viewModel.MapId))
                {
                    this.dialogs[viewModel.MapId].Disappear();
                }
                else
                {
                    this.RemoveRegion(viewModel.MapId);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, string.Format("Cannot close view model '{0}' (mapId='{1}').", viewModel.GetType().Name, viewModel.MapId));
            }
        }

        public string GetNewViewModelName(string fullViewName)
        {
            var viewModelBind = this.GetViewModelBind(fullViewName);
            if (viewModelBind != null)
            {
                var instanceModuleViewName = GetNewModuleViewName(viewModelBind, fullViewName);
                this.RegisterType(viewModelBind, instanceModuleViewName);
                return instanceModuleViewName;
            }

            return null;
        }

        public INavigableView GetRegisteredView(string instanceModuleViewName)
        {
            return this.regionManager.Regions[instanceModuleViewName].ActiveViews.First() as INavigableView;
        }

        public INavigableViewModel GetRegisteredViewModel(string mapId, object data = null)
        {
            var viewModel = GetViewModelByMapId(mapId);
            viewModel.MapId = mapId;
            viewModel.Token = mapId;
            viewModel.Data = data;
            return viewModel;
        }

        public INavigableView GetNewView(string moduleName, string viewModelName, object data)
        {
            if (string.IsNullOrEmpty(viewModelName))
            {
                return null;
            }

            var modelName = MvvmNaming.GetModelNameFromViewModelName(viewModelName);
            var moduleViewName = MvvmNaming.GetViewName(moduleName, modelName);
            var instanceModuleViewName = this.GetNewViewModelName(moduleViewName);
            return this.GetView(instanceModuleViewName, data);
        }

        public INavigableViewModel GetViewModelByName(string viewModelName)
        {
            if (!MvvmNaming.IsViewModelNameValid(viewModelName))
            {
                return null;
            }

            var names = MvvmNaming.GetViewModelNames(viewModelName);
            return ServiceLocator.Current.GetInstance<INavigableViewModel>(names.viewModelName);
        }

        public INavigableViewModel GetViewModelFromActiveWindow()
        {
            var activeDialog = this.dialogs.Values.Cast<Window>().FirstOrDefault(d => d.IsActive);
            if (activeDialog != null)
            {
                return activeDialog.DataContext as INavigableViewModel;
            }

            if (WmsMainDockLayoutManager.GetActiveView() is ContentControl activeView)
            {
                if (activeView.Content is IWmsHistoryView historyView)
                {
                    return historyView.GetCurrentViewModel();
                }
                else if (activeView is INavigableView navigableView)
                {
                    return navigableView.DataContext as INavigableViewModel;
                }
            }

            return null;
        }

        public void IsBusy(bool value)
        {
            WmsMainDockLayoutManager.IsBusy(value);
        }

        public void LoadModule(string moduleName)
        {
            var catalog = this.container.Resolve<IModuleCatalog>();
            var module = catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName);
            if (module.State != ModuleState.NotStarted)
            {
                return;
            }

            var moduleManager = this.container.Resolve<IModuleManager>();
            moduleManager.LoadModule(moduleName);
        }

        public void Register<TItemsView, TItemsViewModel>()
            where TItemsViewModel : INavigableViewModel
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
        public INavigableViewModel RegisterAndGetViewModel(string viewName, string token, object data = null)
        {
            if (!this.registrations.ContainsKey(viewName))
            {
                throw new InvalidOperationException(
                    $"Before invoking the ({nameof(this.RegisterAndGetViewModel)}) method, the {nameof(this.Register)} method needs to be called " +
                    "in order to register an association between the specified view and a view model type.");
            }

            var viewModelBind = this.registrations[viewName];

            // Generate random mapId
            var mapId = Guid.NewGuid().ToString("N");
            this.container.RegisterType(typeof(INavigableViewModel), viewModelBind.ViewModel, mapId);
            var vm = ServiceLocator.Current.GetInstance<INavigableViewModel>(mapId);
            vm.Token = token;
            return vm;
        }

        public void StartPresentation(Action operationBefore, Action operationAfter)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.SystemIdle,
                new Action(() =>
                {
                    var inputService = ServiceLocator.Current.GetInstance<IInputService>();

                    // Get current UI context
                    if (inputService.FocusedElement is UIElement elementFocused)
                    {
                        elementFocused.Dispatcher.BeginInvoke(
                             DispatcherPriority.DataBind,
                             new Action(() =>
                             {
                                 operationBefore?.Invoke();
                             }));
                        elementFocused.Dispatcher.BeginInvoke(
                            DispatcherPriority.SystemIdle,
                            new Action(() =>
                            {
                                Application.Current.MainWindow.Show();
                                operationAfter.Invoke();
                            }));
                    }
                }));
        }

        private static string GetNewModuleViewName(ViewModelBind viewModelBind, string moduleViewName)
        {
            var newRegId = viewModelBind.GetNewId();
            return $"{moduleViewName}.{newRegId}";
        }

        private static INavigableViewModel GetViewModelByMapId(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                throw new ArgumentException("The argument cannot be null or empty.", nameof(mapId));
            }

            return ServiceLocator.Current.GetInstance<INavigableViewModel>(mapId);
        }

        private INavigableView AddToRegion(string moduleViewName, object data)
        {
            this.lastViewActivated = this.GetView(moduleViewName, data);
            if (this.lastViewActivated.ViewType == WmsViewType.Docking)
            {
                WmsMainDockLayoutManager.Current.RegisterView(moduleViewName, this.lastViewActivated.Title, this.lastViewActivated, this.regionManager);
                this.regionManager.AddToRegion(moduleViewName, this.lastViewActivated);
            }
            else
            {
                this.RegisterDialog(moduleViewName, this.lastViewActivated);
            }

            return this.lastViewActivated;
        }

        private INavigableView CheckAddRegion(string moduleViewName, object data)
        {
            var viewModelBind = this.GetViewModelBind(moduleViewName);
            var instanceModuleViewName = $"{moduleViewName}.{viewModelBind.Ids.First()}";
            if (!this.regionManager.Regions.ContainsRegionWithName(instanceModuleViewName))
            {
                // Map Prism region to current layout
                return this.AddToRegion(instanceModuleViewName, data);
            }

            // Register new instance of same view type
            instanceModuleViewName = GetNewModuleViewName(viewModelBind, moduleViewName);
            this.RegisterType(viewModelBind, instanceModuleViewName);

            // Map cloned type to current layout
            return this.AddToRegion(instanceModuleViewName, data);
        }

        private string GetNewRegistrationId<TItemsView, TItemsViewModel>()
            where TItemsView : INavigableView
            where TItemsViewModel : INavigableViewModel
        {
            string newId = null;
            ViewModelBind viewModelBind = null;
            var fullViewName = typeof(TItemsView).ToString();
            if (!this.registrations.ContainsKey(fullViewName))
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

        private void RegisterDialog(string moduleViewName, INavigableView registeredView)
        {
            this.dialogs.Add(moduleViewName, registeredView);
            WmsDialogView.ShowDialog(registeredView, this.IsUnitTest);
        }

        private void RegisterType(ViewModelBind viewModelBind, string instanceModuleViewName)
        {
            this.container.RegisterType(typeof(INavigableViewModel), viewModelBind.ViewModel, instanceModuleViewName);
            this.container.RegisterType(typeof(INavigableView), viewModelBind.View, instanceModuleViewName);
        }

        private void RemoveRegion(string moduleRegionName)
        {
            if (this.regionManager.Regions.ContainsRegionWithName(moduleRegionName) == false)
            {
                return;
            }

            var region = this.regionManager.Regions[moduleRegionName];
            var viewActive = this.regionManager.Regions[moduleRegionName].ActiveViews.First();
            (viewActive as INavigableView)?.Disappear();
            region.Deactivate(viewActive);
            this.regionManager.Regions[moduleRegionName].Remove(viewActive);
            this.regionManager.Regions.Remove(moduleRegionName);
        }

        #endregion

        #region Classes

        private class ViewModelBind
        {
            #region Constructors

            public ViewModelBind(Type view, Type viewModel)
            {
                this.View = view;
                this.ViewModel = viewModel;
                this.Ids = new List<string>();
            }

            #endregion

            #region Properties

            public List<string> Ids { get; set; }

            public Type View { get; set; }

            public Type ViewModel { get; set; }

            #endregion

            #region Methods

            public string GetNewId()
            {
                var newId = (this.Ids.Count + 1).ToString();
                this.Ids.Add(newId);
                return newId;
            }

            #endregion
        }

        #endregion
    }
}
