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
    public class CustomControlDrawerWeightSaturationDataGridViewModel : BaseViewModel, ICustomControlDrawerWeightSaturationDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridDrawerWeightSaturation> drawers;

        private DataGridDrawerWeightSaturation selectedDrawer;

        #endregion

        #region Properties

        public ObservableCollection<DataGridDrawerWeightSaturation> Drawers { get => this.drawers; set => this.SetProperty(ref this.drawers, value); }

        public DataGridDrawerWeightSaturation SelectedDrawer { get => this.selectedDrawer; set => this.SetProperty(ref this.selectedDrawer, value); }

        #endregion
    }
}
