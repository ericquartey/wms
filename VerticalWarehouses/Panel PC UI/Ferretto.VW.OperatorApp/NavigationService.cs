using System.Collections.Generic;
using Ferretto.VW.CustomControls;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.Utils.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp
{
    public class NavigationService : INavigationService
    {
        #region Fields

        public static Stack<BindableBase> navigationStackTrace;

        private static IUnityContainer _container;

        private static IEventAggregator _eventAggregator;

        private static IMainWindowBackToOAPPButtonViewModel footerViewModel;

        private static IMainWindowViewModel mainWindowViewModel;

        #endregion

        #region Constructors

        public NavigationService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            navigationStackTrace = new Stack<BindableBase>();
        }

        #endregion

        #region Methods

        public static void NavigateFromView()
        {
            if (navigationStackTrace.Count != 0 && navigationStackTrace.Pop() is IViewModel destinationViewModel)
            {
                mainWindowViewModel.ContentRegionCurrentViewModel = destinationViewModel as BindableBase;
                destinationViewModel.OnEnterViewAsync();
                if (destinationViewModel is IViewModel destination && destination.NavigationViewModel != null)
                {
                    mainWindowViewModel.NavigationRegionCurrentViewModel = destination.NavigationViewModel;
                }
                else
                {
                    mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                }
            }
            else
            {
                mainWindowViewModel.ContentRegionCurrentViewModel = _container.Resolve<IIdleViewModel>() as IdleViewModel;
                mainWindowViewModel.NavigationRegionCurrentViewModel = _container.Resolve<IMainWindowNavigationButtonsViewModel>() as MainWindowNavigationButtonsViewModel;
                mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = null;
            }
        }

        public static async void NavigateToView<T, I>()
                    where T : BindableBase, I
            where I : IViewModel
        {
            if (_container.Resolve<I>() is T desiredViewModelWithNavView && desiredViewModelWithNavView.NavigationViewModel != null)
            {
                if (!(mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                {
                    navigationStackTrace.Push(mainWindowViewModel.ContentRegionCurrentViewModel);
                }
                await desiredViewModelWithNavView.OnEnterViewAsync();
                mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModelWithNavView;
                mainWindowViewModel.NavigationRegionCurrentViewModel = desiredViewModelWithNavView.NavigationViewModel;
                mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>() as MainWindowBackToOAPPButtonViewModel;
            }
            else if (_container.Resolve<I>() is T desiredViewModel)
            {
                if (!(mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel)) navigationStackTrace.Push(mainWindowViewModel.ContentRegionCurrentViewModel);
                await desiredViewModel.OnEnterViewAsync();
                mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>() as MainWindowBackToOAPPButtonViewModel;
            }
        }

        public static async void NavigateToView<T, I>(object parameterObject)
                    where T : BindableBase, I
            where I : IViewModel
        {
            if (parameterObject is TestArticle article)
            {
                if (_container.Resolve<I>() is ItemDetailViewModel desiredViewModel)
                {
                    if (!(mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel)) navigationStackTrace.Push(mainWindowViewModel.ContentRegionCurrentViewModel);
                    desiredViewModel.Article = article;
                    await desiredViewModel.OnEnterViewAsync();
                    mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                    mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                    mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>() as MainWindowBackToOAPPButtonViewModel;
                }
            }
        }

        public static async void NavigateToViewWithoutNavigationStack<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            if (_container.Resolve<I>() is T desiredViewModelWithNavView && desiredViewModelWithNavView.NavigationViewModel != null)
            {
                await desiredViewModelWithNavView.OnEnterViewAsync();
                mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModelWithNavView;
                mainWindowViewModel.NavigationRegionCurrentViewModel = desiredViewModelWithNavView.NavigationViewModel;
                mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>() as MainWindowBackToOAPPButtonViewModel;
            }
            else if (_container.Resolve<I>() is T desiredViewModel)
            {
                await desiredViewModel.OnEnterViewAsync();
                mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>() as MainWindowBackToOAPPButtonViewModel;
            }
        }

        public void Initialize(IUnityContainer container)
        {
            _container = container;
            mainWindowViewModel = _container.Resolve<IMainWindowViewModel>();
            footerViewModel = _container.Resolve<IMainWindowBackToOAPPButtonViewModel>();
        }

        #endregion
    }
}
