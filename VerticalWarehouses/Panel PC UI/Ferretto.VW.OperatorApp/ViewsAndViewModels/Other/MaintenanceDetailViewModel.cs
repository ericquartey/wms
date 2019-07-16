using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class MaintenanceDetailViewModel : BaseViewModel, IMaintenanceDetailViewModel
    {
        #region Fields

        private readonly CustomControlMaintenanceDataGridViewModel dataGridViewModelRef;

        private readonly DataGridKit selectedKit;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridKit> kits;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(
            ICustomControlMaintenanceDataGridViewModel maintenanceDataGridViewModel)
        {
            if (maintenanceDataGridViewModel == null)
            {
                throw new ArgumentNullException(nameof(maintenanceDataGridViewModel));
            }

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

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
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
        }

        #endregion
    }
}
