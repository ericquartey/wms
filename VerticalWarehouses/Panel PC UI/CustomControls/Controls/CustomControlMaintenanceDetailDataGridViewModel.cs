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
    public class CustomControlMaintenanceDetailDataGridViewModel : BindableBase, ICustomControlMaintenanceDetailDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridMaintenanceDetail> maintenanceDetails;

        private DataGridMaintenanceDetail selectedMaintenanceDetail;

        #endregion

        #region Properties

        public ObservableCollection<DataGridMaintenanceDetail> MaintenanceDetails { get => this.maintenanceDetails; set => this.SetProperty(ref this.maintenanceDetails, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridMaintenanceDetail SelectedMaintenanceDetail { get => this.selectedMaintenanceDetail; set => this.SetProperty(ref this.selectedMaintenanceDetail, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // HACK
        }

        public Task OnEnterViewAsync()
        {
            // HACK
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // HACK
        }

        #endregion
    }
}
