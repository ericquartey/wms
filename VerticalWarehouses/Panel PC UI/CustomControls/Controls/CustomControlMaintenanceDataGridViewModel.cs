using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlMaintenanceDataGridViewModel : BaseViewModel, ICustomControlMaintenanceDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridKit> kits;

        private DataGridKit selectedKit;

        #endregion

        #region Properties

        public ObservableCollection<DataGridKit> Kits { get => this.kits; set => this.SetProperty(ref this.kits, value); }

        public DataGridKit SelectedKit { get => this.selectedKit; set => this.SetProperty(ref this.selectedKit, value); }

        #endregion
    }
}
