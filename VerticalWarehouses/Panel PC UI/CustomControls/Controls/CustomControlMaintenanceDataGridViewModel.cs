using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlMaintenanceDataGridViewModel : BindableBase, ICustomControlMaintenanceDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridKit> kits;

        private DataGridKit selectedKit;

        #endregion

        #region Properties

        public ObservableCollection<DataGridKit> Kits { get => this.kits; set => this.SetProperty(ref this.kits, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridKit SelectedKit { get => this.selectedKit; set => this.SetProperty(ref this.selectedKit, value); }

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
