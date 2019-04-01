using System.Windows.Input;
using Ferretto.VW.OperatorApp.Resources;
using Ferretto.VW.OperatorApp.Resources.Enumerations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.Utils.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists;

namespace Ferretto.VW.OperatorApp
{
    public partial class MainWindowViewModel
    {
        #region Fields

        private ICommand backToMainWindowNavigationButtonsViewCommand;

        private ICommand backToVWAPPCommand;

        private ICommand drawerActivityButtonCommand;

        private ICommand itemSearchButtonCommand;

        private ICommand listsInWaitButtonCommand;

        private ICommand machineModeCustomCommand;

        private ICommand machineOnMarchCustomCommand;

        private ICommand openHelpWindow;

        #endregion

        #region Properties

        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ??
            (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() =>
        {
            this.ChangeNavigationRegion<MainWindowNavigationButtonsViewModel, IMainWindowNavigationButtonsViewModel>();
            this.NavigateToView<IdleViewModel, IIdleViewModel>();
            this.eventAggregator.GetEvent<OperatorApp_Event>().Publish(new OperatorApp_EventMessage(OperatorApp_EventMessageType.ExitView));
        }));

        public ICommand BackToVWAPPCommand => this.backToVWAPPCommand ?? (this.backToVWAPPCommand = new DelegateCommand(() =>
        {
            this.IsPopupOpen = false;
            this.eventAggregator.GetEvent<OperatorApp_Event>().Publish(new OperatorApp_EventMessage(OperatorApp_EventMessageType.BackToVWApp));
            ClickedOnMachineModeEventHandler = null;
            ClickedOnMachineOnMarchEventHandler = null;
        }));

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand ?? (this.drawerActivityButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToView<DrawerOperationsMainViewModel, IDrawerOperationsMainViewModel>();
        }));

        public ICommand ItemSearchButtonCommand => this.itemSearchButtonCommand ?? (this.itemSearchButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToView<ItemSearchViewModel, IItemSearchViewModel>();
        }));

        public ICommand ListsInWaitButtonCommand => this.listsInWaitButtonCommand ?? (this.listsInWaitButtonCommand = new DelegateCommand(() =>
        {
            this.NavigateToView<ListsInWaitViewModel, IListsInWaitViewModel>();
        }));

        public ICommand MachineModeCustomCommand => this.machineModeCustomCommand ?? (this.machineModeCustomCommand = new DelegateCommand(() => RaiseClickedOnMachineModeEvent()));

        public ICommand MachineOnMarchCustomCommand => this.machineOnMarchCustomCommand ?? (this.machineOnMarchCustomCommand = new DelegateCommand(() => RaiseClickedOnMachineOnMarchEvent()));

        public ICommand OpenHelpWindow => this.openHelpWindow ?? (this.openHelpWindow = new DelegateCommand(() =>
        {
            this.helpWindow.Show();
            this.helpWindow.HelpContentRegion.Content = this.contentRegionCurrentViewModel;
        }));

        #endregion

        #region Methods

        private void NavigateToView<T, I>()
            where T : BindableBase, I
            where I : IViewModel
        {
            this.eventAggregator.GetEvent<OperatorApp_Event>().Publish(new OperatorApp_EventMessage(OperatorApp_EventMessageType.EnterView));
            var desiredViewModel = this.container.Resolve<I>() as T;
            desiredViewModel.SubscribeMethodToEvent();
            this.container.Resolve<IMainWindowBackToOAPPButtonViewModel>().BackButtonCommand.RegisterCommand(new DelegateCommand(desiredViewModel.ExitFromViewMethod));
            this.ContentRegionCurrentViewModel = desiredViewModel;
        }

        #endregion
    }
}
