using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other.Statistics
{
    public class StatisticsNavigationViewModel : BindableBase, IStatisticsNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private ICommand cellsStatisticsButtonCommand;

        private IUnityContainer container;

        private ICommand drawerSpaceSaturationButtonCommand;

        private ICommand errorsStatisticsButtonCommand;

        private ICommand machineStatisticsButtonCommand;

        #endregion

        #region Constructors

        public StatisticsNavigationViewModel(
            IEventAggregator eventAggregator,
            INavigationService navigationService)
        {
            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand CellsStatisticsButtonCommand => this.cellsStatisticsButtonCommand ?? (this.cellsStatisticsButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<CellsStatisticsViewModel, ICellsStatisticsViewModel>();
        }));

        public ICommand DrawerSpaceSaturationButtonCommand => this.drawerSpaceSaturationButtonCommand ?? (this.drawerSpaceSaturationButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<DrawerSpaceSaturationViewModel, IDrawerSpaceSaturationViewModel>();
        }));

        public ICommand ErrorsStatisticsButtonCommand => this.errorsStatisticsButtonCommand ?? (this.errorsStatisticsButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<ErrorsStatisticsViewModel, IErrorsStatisticsViewModel>();
        }));

        public ICommand MachineStatisticsButtonCommand => this.machineStatisticsButtonCommand ?? (this.machineStatisticsButtonCommand = new DelegateCommand(() =>
                                {
                                    this.navigationService.NavigateToView<MachineStatisticsViewModel, IMachineStatisticsViewModel>();
                                }));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
