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
    public class CustomControlDrawerWeightSaturationDataGridViewModel : BindableBase, ICustomControlDrawerWeightSaturationDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridDrawerWeightSaturation> drawers;

        private DataGridDrawerWeightSaturation selectedDrawer;

        #endregion

        #region Properties

        public ObservableCollection<DataGridDrawerWeightSaturation> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridDrawerWeightSaturation SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

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
