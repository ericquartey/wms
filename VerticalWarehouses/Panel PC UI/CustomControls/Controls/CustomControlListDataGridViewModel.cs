using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlListDataGridViewModel : BaseViewModel, ICustomControlListDataGridViewModel
    {
        #region Fields

        private IEnumerable<DataGridList> lists;

        private DataGridList selectedList;

        #endregion

        #region Properties

        public IEnumerable<DataGridList> Lists { get => this.lists; set => this.SetProperty(ref this.lists, value); }

        public DataGridList SelectedList { get => this.selectedList; set => this.SetProperty(ref this.selectedList, value); }

        #endregion
    }
}
