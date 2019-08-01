//Header test C#

using System.Collections.ObjectModel;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;

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
