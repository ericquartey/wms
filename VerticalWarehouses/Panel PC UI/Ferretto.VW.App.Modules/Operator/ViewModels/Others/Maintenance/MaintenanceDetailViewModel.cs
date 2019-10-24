using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class MaintenanceDetailViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ICustomControlMaintenanceDetailDataGridViewModel customControlMaintenanceDetail;

        private readonly CustomControlMaintenanceDetailDataGridViewModel dataGridViewModelRef;

        private BindableBase dataGridViewModel;

        private ObservableCollection<DataGridMaintenanceDetail> maintenanceDetails;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(
            ICustomControlMaintenanceDetailDataGridViewModel customControlMaintenanceDetail)
            : base(PresentationMode.Operator)
        {
            this.customControlMaintenanceDetail = customControlMaintenanceDetail ?? throw new ArgumentNullException(nameof(customControlMaintenanceDetail));

            this.dataGridViewModelRef = customControlMaintenanceDetail as CustomControlMaintenanceDetailDataGridViewModel;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public override EnableMask EnableMask => EnableMask.None;

        public ICustomControlMaintenanceDetailDataGridViewModel MaintenanceDataGridViewModel { get; }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

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
        }

        #endregion
    }
}
