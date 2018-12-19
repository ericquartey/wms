using System;
using Prism.Mvvm;
using Ferretto.VW.OperatorApp.ViewsAndViewModels;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Articles;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Lists;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Picking;
using Ferretto.VW.Navigation;
using System.Windows.Input;
using Prism.Commands;

namespace Ferretto.VW.OperatorApp
{
    internal class MainWindowViewModel : BindableBase
    {
        #region Fields

        private readonly ArticleDetailsViewModel ArticleDetailsVMInstance = new ArticleDetailsViewModel();
        private readonly ArticlesNavigationButtonViewModel ArticlesNavigationButtonVMInstance = new ArticlesNavigationButtonViewModel();
        private readonly ArticlesViewModel ArticlesVMInstance = new ArticlesViewModel();
        private readonly ListDetailsViewModel ListDetailsVMInstance = new ListDetailsViewModel();
        private readonly ListsNavigationButtonViewModel ListsNavigationButtonVMInstance = new ListsNavigationButtonViewModel();
        private readonly ListsViewModel ListsVMInstance = new ListsViewModel();
        private readonly NavigationButtonViewModel NavigationButtonVMInstance = new NavigationButtonViewModel();
        private readonly PickingDetailsViewModel PickingDetailsVMInstance = new PickingDetailsViewModel();
        private readonly PickingNavigationButtonViewModel PickingNavigationButtonVMInstance = new PickingNavigationButtonViewModel();
        private readonly PickingViewModel PickinVMInstance = new PickingViewModel();

        private ICommand articleDetailsButtonCommand;
        private ICommand articlesButtonCommand;
        private ICommand backToMainWindowNavigationButtonsViewCommand;
        private BindableBase contentRegionCurrentViewModel;
        private bool machineModeSelectionBool = true;
        private int machineModeSelectionInt;
        private bool machineOnMarchSelectionBool = false;
        private int machineOnMarchSelectionInt;
        private BindableBase navigationRegionCurrentViewModel;

        #endregion Fields

        #region Constructors

        public MainWindowViewModel()
        {
            this.NavigationRegionCurrentViewModel = this.NavigationButtonVMInstance;
        }

        #endregion Constructors

        #region Properties

        public ICommand ArticleDetailsButtonCommand => this.articleDetailsButtonCommand ?? (this.articleDetailsButtonCommand = new DelegateCommand(() => this.ContentRegionCurrentViewModel = this.ArticleDetailsVMInstance));
        public ICommand ArticlesButtonCommand => this.articlesButtonCommand ?? (this.articlesButtonCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.ArticlesNavigationButtonVMInstance; this.ContentRegionCurrentViewModel = this.ArticlesVMInstance; }));
        public ICommand BackToMainWindowNavigationButtonsViewButtonCommand => this.backToMainWindowNavigationButtonsViewCommand ?? (this.backToMainWindowNavigationButtonsViewCommand = new DelegateCommand(() => { this.NavigationRegionCurrentViewModel = this.NavigationButtonVMInstance; this.ContentRegionCurrentViewModel = null; NavigationService.RaiseExitViewEvent(); }));
        public BindableBase ContentRegionCurrentViewModel { get => this.contentRegionCurrentViewModel; set => this.SetProperty(ref this.contentRegionCurrentViewModel, value); }
        public Boolean MachineModeSelectionBool { get => this.machineModeSelectionBool; set => this.SetProperty(ref this.machineModeSelectionBool, value); }
        public Int32 MachineModeSelectionInt { get => this.machineModeSelectionInt; set { this.SetProperty(ref this.machineModeSelectionInt, value); this.MachineModeSelectionBool = this.machineModeSelectionInt == 0 ? true : false; } }
        public Boolean MachineOnMarchSelectionBool { get => this.machineOnMarchSelectionBool; set => this.SetProperty(ref this.machineOnMarchSelectionBool, value); }
        public Int32 MachineOnMarchSelectionInt { get => this.machineOnMarchSelectionInt; set { this.SetProperty(ref this.machineOnMarchSelectionInt, value); this.MachineOnMarchSelectionBool = this.machineOnMarchSelectionInt == 0 ? false : true; } }
        public BindableBase NavigationRegionCurrentViewModel { get => this.navigationRegionCurrentViewModel; set => this.SetProperty(ref this.navigationRegionCurrentViewModel, value); }

        #endregion Properties
    }
}
