using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.Other
{
    public class MaintenanceMainPageViewModel : BaseViewModel, IMaintenanceMainPageViewModel
    {
        #region Fields

        private readonly CustomControlMaintenanceDataGridViewModel dataGridViewModelRef;

        private readonly INavigationService navigationService;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridKit> kits;

        private ICommand maintenanceDetailButtonCommand;

        #endregion

        #region Constructors

        public MaintenanceMainPageViewModel(
            ICustomControlMaintenanceDataGridViewModel maintenanceDataGridViewModel,
            INavigationService navigationService)
        {
            if (maintenanceDataGridViewModel == null)
            {
                throw new ArgumentNullException(nameof(maintenanceDataGridViewModel));
            }

            if (navigationService == null)
            {
                throw new ArgumentNullException(nameof(navigationService));
            }

            this.navigationService = navigationService;
            this.MaintenanceDataGridViewModel = maintenanceDataGridViewModel;
            this.dataGridViewModelRef = maintenanceDataGridViewModel as CustomControlMaintenanceDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public ICustomControlMaintenanceDataGridViewModel MaintenanceDataGridViewModel { get; }

        public ICommand MaintenanceDetailButtonCommand =>
                    this.maintenanceDetailButtonCommand
            ??
            (this.maintenanceDetailButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<MaintenanceDetailViewModel, IMaintenanceDetailViewModel>();
                }));

        #endregion

        #region Methods

        public override Task OnEnterViewAsync()
        {
            var random = new Random();
            this.kits = new ObservableCollection<DataGridKit>();
            for (var i = 0; i < random.Next(3, 30); i++)
            {
                this.kits.Add(new DataGridKit
                {
                    Kit = $"Kit {i}",
                    Description = $"Kit number {i}",
                    State = $"State",
                    Request = "Request"
                }
                );
            }
            this.dataGridViewModelRef.Kits = this.kits;
            this.dataGridViewModelRef.SelectedKit = this.kits[0];
            this.DataGridViewModel = this.dataGridViewModelRef;

            return Task.CompletedTask;
        }

        #endregion
    }
}
