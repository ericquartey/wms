using System.Collections.Generic;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels;
using Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations.Details;
using Ferretto.VW.App.Operator.ViewsAndViewModels.SearchItem;
using Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists.ListDetail;
using Ferretto.VW.Utils.Interfaces;
using Ferretto.VW.WmsCommunication.Source;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Operator
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly IFooterViewModel footerViewModel;

        private readonly Stack<BindableBase> navigationStack = new Stack<BindableBase>();

        private IMainWindowViewModel mainWindowViewModel;

        #endregion

        #region Constructors

        public NavigationService(
            IEventAggregator eventAggregator,
            IUnityContainer container,
            IFooterViewModel footerViewModel)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (container == null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.eventAggregator = eventAggregator;
            this.container = container;
            this.footerViewModel = footerViewModel;
        }

        #endregion

        #region Methods

        public void NavigateFromView()
        {
            this.mainWindowViewModel = this.mainWindowViewModel ?? this.container.Resolve<IMainWindowViewModel>();

            if (this.navigationStack.Count != 0 && this.navigationStack.Pop() is IViewModel destinationViewModel)
            {
                if (this.mainWindowViewModel.ContentRegionCurrentViewModel is IViewModel vm)
                {
                    vm.ExitFromViewMethod();
                }
                destinationViewModel.OnEnterViewAsync();
                this.mainWindowViewModel.ContentRegionCurrentViewModel = destinationViewModel as BindableBase;
                if (destinationViewModel is IViewModel destination && destination.NavigationViewModel != null)
                {
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = destination.NavigationViewModel;
                }
                else
                {
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                }
            }
            else
            {
                if (this.mainWindowViewModel.ContentRegionCurrentViewModel is IViewModel vm)
                {
                    vm.ExitFromViewMethod();
                }
                this.mainWindowViewModel.ContentRegionCurrentViewModel = this.container.Resolve<IIdleViewModel>() as IdleViewModel;
                this.mainWindowViewModel.NavigationRegionCurrentViewModel = this.container.Resolve<IMainWindowNavigationButtonsViewModel>() as MainWindowNavigationButtonsViewModel;
                this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = null;
            }
        }

        public async void NavigateToView<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.mainWindowViewModel =
                this.mainWindowViewModel
                ??
                this.container.Resolve<IMainWindowViewModel>();

            var viewModel = this.container.Resolve<I>();

            if (viewModel is T desiredViewModelWithNavView
                &&
                desiredViewModelWithNavView.NavigationViewModel != null)
            {
                if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                {
                    this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                }
                await desiredViewModelWithNavView.OnEnterViewAsync();
                this.mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModelWithNavView;
                this.mainWindowViewModel.NavigationRegionCurrentViewModel = desiredViewModelWithNavView.NavigationViewModel;
                this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
            }
            else if (this.container.Resolve<I>() is T desiredViewModel)
            {
                if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                {
                    this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                }
                await desiredViewModel.OnEnterViewAsync();
                this.mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
            }
        }

        public async void NavigateToView<T, I>(object parameterObject)
            where T : BindableBase, I
            where I : IViewModel
        {
            this.mainWindowViewModel = this.mainWindowViewModel ?? this.container.Resolve<IMainWindowViewModel>();

            if (parameterObject is Item item)
            {
                if (this.container.Resolve<I>() is ItemDetailViewModel desiredViewModel)
                {
                    if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                    {
                        this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                    }
                    desiredViewModel.Item = item;
                    await desiredViewModel.OnEnterViewAsync();
                    this.mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                    this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
                }
            }
            if (parameterObject is DrawerActivityItemDetail itemDetail)
            {
                if (this.container.Resolve<I>() is DrawerActivityInventoryDetailViewModel inventoryViewModel)
                {
                    if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                    {
                        this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                    }
                    inventoryViewModel.ItemDetail = itemDetail;
                    await inventoryViewModel.OnEnterViewAsync();
                    this.mainWindowViewModel.ContentRegionCurrentViewModel = inventoryViewModel;
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                    this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
                }
                else if (this.container.Resolve<I>() is DrawerActivityPickingDetailViewModel pickingViewModel)
                {
                    if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                    {
                        this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                    }
                    pickingViewModel.ItemDetail = itemDetail;
                    await pickingViewModel.OnEnterViewAsync();
                    this.mainWindowViewModel.ContentRegionCurrentViewModel = pickingViewModel;
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                    this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
                }
                else if (this.container.Resolve<I>() is DrawerActivityRefillingDetailViewModel refillingViewModel)
                {
                    if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                    {
                        this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                    }
                    refillingViewModel.ItemDetail = itemDetail;
                    await refillingViewModel.OnEnterViewAsync();
                    this.mainWindowViewModel.ContentRegionCurrentViewModel = refillingViewModel;
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                    this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
                }
            }

            if (parameterObject is DataGridList list)
            {
                if (this.container.Resolve<I>() is DetailListInWaitViewModel detailListViewModel)
                {
                    if (!(this.mainWindowViewModel.ContentRegionCurrentViewModel is IdleViewModel))
                    {
                        this.navigationStack.Push(this.mainWindowViewModel.ContentRegionCurrentViewModel);
                    }
                    detailListViewModel.List = list;
                    await detailListViewModel.OnEnterViewAsync();
                    this.mainWindowViewModel.ContentRegionCurrentViewModel = detailListViewModel;
                    this.mainWindowViewModel.NavigationRegionCurrentViewModel = detailListViewModel.NavigationViewModel;
                    this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
                }
            }
        }

        public async void NavigateToViewWithoutNavigationStack<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.mainWindowViewModel = this.mainWindowViewModel ?? this.container.Resolve<IMainWindowViewModel>();

            if (this.container.Resolve<I>() is T desiredViewModelWithNavView && desiredViewModelWithNavView.NavigationViewModel != null)
            {
                await desiredViewModelWithNavView.OnEnterViewAsync();
                this.mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModelWithNavView;
                this.mainWindowViewModel.NavigationRegionCurrentViewModel = desiredViewModelWithNavView.NavigationViewModel;
                this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
            }
            else if (this.container.Resolve<I>() is T desiredViewModel)
            {
                await desiredViewModel.OnEnterViewAsync();
                this.mainWindowViewModel.ContentRegionCurrentViewModel = desiredViewModel;
                this.mainWindowViewModel.NavigationRegionCurrentViewModel = null;
                this.mainWindowViewModel.ExitViewButtonRegionCurrentViewModel = this.footerViewModel as BindableBase;
            }
        }

        #endregion
    }
}
