//Header test C#
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlMaintenanceDetailDataGridViewModel : BaseViewModel, ICustomControlMaintenanceDetailDataGridViewModel
    {
        #region Private Fields

        private ObservableCollection<DataGridMaintenanceDetail> maintenanceDetails;

        private DataGridMaintenanceDetail selectedMaintenanceDetail;

        #endregion

        #region Public Properties

        public ObservableCollection<DataGridMaintenanceDetail> MaintenanceDetails { get => this.maintenanceDetails; set => this.SetProperty(ref this.maintenanceDetails, value); }

        public DataGridMaintenanceDetail SelectedMaintenanceDetail { get => this.selectedMaintenanceDetail; set => this.SetProperty(ref this.selectedMaintenanceDetail, value); }

        #endregion
    }
}
