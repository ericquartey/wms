using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.CustomControls.Controls;
using System.Collections.ObjectModel;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.CustomControls.Interfaces;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class MaintenanceMainPageViewModel : BindableBase, IMaintenanceMainPageViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlMaintenanceDataGridViewModel dataGridViewModelRef;

        private IEventAggregator eventAggregator;

        private ObservableCollection<DataGridKit> kits;

        private ICommand maintenanceDetailButtonCommand;

        private DataGridKit selectedKit;

        #endregion

        #region Constructors

        public MaintenanceMainPageViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICommand MaintenanceDetailButtonCommand => this.maintenanceDetailButtonCommand ?? (this.maintenanceDetailButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<MaintenanceDetailViewModel, IMaintenanceDetailViewModel>();
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
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlMaintenanceDataGridViewModel>() as CustomControlMaintenanceDataGridViewModel;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.kits = new ObservableCollection<DataGridKit>();
            for (int i = 0; i < random.Next(0, 30); i++)
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
