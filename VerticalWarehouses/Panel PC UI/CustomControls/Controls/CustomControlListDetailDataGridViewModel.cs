using System.Collections.Generic;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlListDetailDataGridViewModel : BaseViewModel, ICustomControlListDetailDataGridViewModel
    {
        #region Fields

        private IEnumerable<DataGridListDetail> lists;

        private DataGridListDetail selectedList;

        #endregion

        #region Properties

        public IEnumerable<DataGridListDetail> Lists { get => this.lists; set => this.SetProperty(ref this.lists, value); }

        public DataGridListDetail SelectedList { get => this.selectedList; set => this.SetProperty(ref this.selectedList, value); }

        #endregion
    }
}
