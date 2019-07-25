using System.Collections.ObjectModel;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;

namespace Ferretto.VW.App.Controls.Controls
{
    public class CustomControlListDetailDataGridViewModel : BaseViewModel, ICustomControlListDetailDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridListDetail> lists;

        private DataGridListDetail selectedList;

        #endregion

        #region Properties

        public ObservableCollection<DataGridListDetail> Lists { get => this.lists; set => this.SetProperty(ref this.lists, value); }

        public DataGridListDetail SelectedList { get => this.selectedList; set => this.SetProperty(ref this.selectedList, value); }

        #endregion
    }
}
