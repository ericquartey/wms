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
    public class CustomControlListDataGridViewModel : BindableBase, ICustomControlListDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridList> lists;

        private DataGridList selectedList;

        #endregion

        #region Properties

        public ObservableCollection<DataGridList> Lists { get => this.lists; set => this.SetProperty(ref this.lists, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridList SelectedList { get => this.selectedList; set => this.SetProperty(ref this.selectedList, value); }

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
