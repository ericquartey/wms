using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Controls
{
    public class CustomControlDrawerSaturationDataGridViewModel : BindableBase, ICustomControlDrawerSaturationDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridDrawerSaturation> drawers;

        private DataGridDrawerSaturation selectedDrawer;

        #endregion

        #region Properties

        public ObservableCollection<DataGridDrawerSaturation> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridDrawerSaturation SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

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
