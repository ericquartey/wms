using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ferretto.Common.Controls.Interfaces;
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
    public void Appear(Uri uri)
    {
      var (module, viewModelName) = GetViewModelNameSplitted(uri);
      this.Appear(module, viewModelName);
    }

    public void Appear<TViewModel>()
    {
      var (module, viewModelName) = GetViewModelNames<TViewModel>();
      this.Appear(module, viewModelName);
    }

    public void Appear(string module, string viewModelName)
    {

        if (IsViewModelNameValid(viewModelName) == false)
        {
          return;
        }

        this.LoadModule(module);

        var name = GetName(viewModelName);
        var moduleViewName = GetViewName(module, name);

        var instanceModuleViewName = this.CheckAddRegion(moduleViewName);

        var region = this.regionManager.Regions[instanceModuleViewName];
        var view = region.Views.FirstOrDefault(v => v.GetType().ToString().Equals(moduleViewName, StringComparison.InvariantCulture));
        region.Activate(view);
    }

    private string CheckAddRegion(string moduleViewName)
    {
      var viewModelBind = this.GetViewModdelBind(moduleViewName);
      var instanceModuleViewName = $"{moduleViewName}.{viewModelBind.Ids.First()}";
      if (this.regionManager.Regions.ContainsRegionWithName(instanceModuleViewName) == false)
      {
        // Map Prism region to current layout
        this.AddToregion(instanceModuleViewName);
        return instanceModuleViewName;
      }

      var idStateNotChanged = GetStateNotChanged(moduleViewName, viewModelBind);
      if (idStateNotChanged != null)
      {
        // View state is not chaaged, activate this id
        instanceModuleViewName = $"{moduleViewName}.{idStateNotChanged}";
      }
      else
      {
        // View state is changed, register new instance of same view type
        var newRegId = viewModelBind.GetNewId();
        instanceModuleViewName = $"{moduleViewName}.{newRegId}";
        this.container.RegisterType(typeof(INavigableViewModel), viewModelBind.ViewModel, instanceModuleViewName);
        this.container.RegisterType(typeof(INavigableView), viewModelBind.View, instanceModuleViewName);
        // Map cloned type to current layout
        this.AddToregion(instanceModuleViewName);
      }

      return instanceModuleViewName;
    }
    #endregion

    #region Disappear

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Major Code Smell",
      "S4144:Methods should not have identical implementations",
      Justification = "Method is not yet fully implemented")]
    public void Disappear(Uri uri)
    {
      var (module, viewModelName) = GetViewModelNameSplitted(uri);
      this.Appear(module, viewModelName);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Major Code Smell",
      "S4144:Methods should not have identical implementations",
      Justification = "Method is not yet fully implemented")]
    public void Disappear<TViewModel>()
    {
      var (module, viewModelName) = GetViewModelNames<TViewModel>();
      this.Appear(module, viewModelName);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Major Code Smell",
      "CA1804",
      Justification = "Method is not yet fully implemented")]
    public void Disappear(string module, string viewModelName)
    {
        if (IsViewModelNameValid(viewModelName) == false)
        {
          return;
        }
        var regionName = GetName(viewModelName);
        var moduleViewName = GetViewName(module, regionName);

        throw new NotSupportedException("Disappear need to be implemented");

        // Get corrent mapid
        var moduleRegionName = $"{module}.{regionName}.1";
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
    }
    #endregion

    public void Register<TItemsView, TItemsViewModel>() where TItemsViewModel : INavigableViewModel
                                                        where TItemsView : INavigableView
    {
      var newRegId = this.GetNewRegistrationId<TItemsView, TItemsViewModel>();
      this.container.RegisterType<INavigableViewModel, TItemsViewModel>(newRegId);
      this.container.RegisterType<INavigableView, TItemsView>(newRegId);
    }

    public INavigableViewModel RegisterAndGetViewModel(string viewName, string token)
    {
      if (this.registrations.ContainsKey(viewName) == false)
      {
        return null;
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
      if (IsViewModelNameValid(viewModelName) == false)
      {
        return null;
      }
      var names = GetViewModelNames(viewModelName);
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
        newId = viewModelBind.GetNewId(); // FIXME this assignment is useless
      }
      newId = $"{typeof(TItemsView)}.{viewModelBind.GetNewId()}";
      return newId;
    }
    private ViewModelBind GetViewModdelBind(string fullViewName)
    {
      if (this.registrations.ContainsKey(fullViewName) == false)
      {
        return null;
      }
      return this.registrations[fullViewName];
    }

    private static string GetStateNotChanged(string moduleViewName, ViewModelBind vmbind)
    {
      foreach (var id in vmbind.Ids)
      {
        var viewModel = ServiceLocator.Current.GetInstance<INavigableViewModel>($"{moduleViewName}.{id}");
        if (string.IsNullOrEmpty(viewModel.StateId))
        {
          return id;
        }
      }
      return null;
    }

    private static bool IsViewModelNameValid(string viewModelName)
    {
      if (string.IsNullOrEmpty(viewModelName))
      {
        return false;
      }

      if (viewModelName.EndsWith(Utils.Common.VIEWMODEL_SUFFIX, System.StringComparison.InvariantCulture) == false)
      {
        return false;
      }

      return true;
    }

    private void AddToregion(string moduleViewName)
    {
      var registeredView = ServiceLocator.Current.GetInstance<INavigableView>(moduleViewName);
      registeredView.Token = moduleViewName;
      registeredView.MapId = moduleViewName;
      WMSMainDockLayoutManager.Current.RegisterView(moduleViewName, registeredView.Title);
      this.regionManager.AddToRegion(moduleViewName, registeredView);
    }

    private static (string module, string viewModelName) GetViewModelNames(string viewModelName)
    {
      return GetViewModelNameSplitted(viewModelName);
    }

    private static (string module, string viewModelName) GetViewModelNames<TViewModel>()
    {
      var type = typeof(TViewModel);
      var viewModelName = type.ToString();
      return GetViewModelNameSplitted(viewModelName);
    }

    private static  (string module, string viewModelName) GetViewModelNameSplitted(string viewModelName)
    {
      var vm = viewModelName.Replace($"{Utils.Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}.", "");
      var vmSplit = vm.Split('.');
      return (vmSplit[0], vmSplit[1]);
    }

    private static (string module, string viewModelName) GetViewModelNameSplitted(Uri uri)
    {
      var vmSplit = uri.ToString().Split('/');
      return (vmSplit[0], vmSplit[1]);
    }

    private static string GetName(string viewModelName)
    {
      return g.Replace(viewModelName, string.Empty);
    }

    static Regex g = new Regex($"{Common.Utils.Common.VIEWMODEL_SUFFIX}$", RegexOptions.Compiled);

    private static string GetViewName(string module, string regionName)
    {
      return $"{Utils.Common.ASSEMBLY_QUALIFIEDNAME_PREFIX}.{module}.{regionName}{Utils.Common.VIEW_SUFFIX}";
    }

    private void LoadModule(string moduleName)
    {
      var catalog = this.container.Resolve<IModuleCatalog>();
      var module = (catalog.Modules.FirstOrDefault(m => m.ModuleName == moduleName));
      if (module.State == ModuleState.NotStarted)
      {
        var moduleManager = this.container.Resolve<IModuleManager>();
        moduleManager.LoadModule(moduleName);
      }
    }
    #endregion

    #region Helper class
    class ViewModelBind
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
        var newId = (this.Ids.Count + 1).ToString();
        this.Ids.Add(newId);
        return newId;
      }
    }
    #endregion
  }
}
