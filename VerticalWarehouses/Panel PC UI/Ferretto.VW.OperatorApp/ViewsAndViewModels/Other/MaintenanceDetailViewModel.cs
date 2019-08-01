using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Operator.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.Other
{
    public class MaintenanceDetailViewModel : BaseViewModel, IMaintenanceDetailViewModel
    {
        #region Fields

        private readonly CustomControlMaintenanceDetailDataGridViewModel dataGridViewModelRef;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridMaintenanceDetail> maintenanceDetails;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(
            ICustomControlMaintenanceDetailDataGridViewModel maintenanceDataGridViewModel)
        {
            if (maintenanceDataGridViewModel == null)
            {
                throw new ArgumentNullException(nameof(maintenanceDataGridViewModel));
            }

            this.MaintenanceDataGridViewModel = maintenanceDataGridViewModel;
            this.dataGridViewModelRef = maintenanceDataGridViewModel as CustomControlMaintenanceDetailDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public ICustomControlMaintenanceDetailDataGridViewModel MaintenanceDataGridViewModel { get; }

        #endregion

        #region Methods

        public override Task OnEnterViewAsync()
        {
            var random = new Random();
            this.maintenanceDetails = new ObservableCollection<DataGridMaintenanceDetail>();
            for (int i = 0; i < random.Next(3, 30); i++)
            {
                this.maintenanceDetails.Add(new DataGridMaintenanceDetail
                {
                    Element = $"Element {i}",
                    Description = $"This is element {i}",
                    Quantity = random.Next(2, 40).ToString(),
                }
                );
            }
            this.dataGridViewModelRef.MaintenanceDetails = this.maintenanceDetails;
            this.dataGridViewModelRef.SelectedMaintenanceDetail = this.maintenanceDetails[0];
            this.DataGridViewModel = this.dataGridViewModelRef;

            return Task.CompletedTask;
        }

        #endregion
    }
}
