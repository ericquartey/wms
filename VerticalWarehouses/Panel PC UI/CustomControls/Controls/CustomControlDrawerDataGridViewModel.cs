using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlDrawerDataGridViewModel : BindableBase, ICustomControlDrawerDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridDrawer> drawers;

        private DataGridDrawer selectedDrawer;

        #endregion

        #region Properties

        public ObservableCollection<DataGridDrawer> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridDrawer SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

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
