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
    public class CustomControlItemStatisticsDataGridViewModel : BindableBase, ICustomControlItemStatisticsDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridItemStatistics> items;

        private DataGridItemStatistics selectedItem;

        #endregion

        #region Properties

        public ObservableCollection<DataGridItemStatistics> Items { get => this.items; set => this.SetProperty(ref this.items, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridItemStatistics SelectedItem { get => this.selectedItem; set => this.SetProperty(ref this.selectedItem, value); }

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
