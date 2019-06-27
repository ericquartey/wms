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
    public class CustomControlCellStatisticsDataGridViewModel : BindableBase, ICustomControlCellStatisticsDataGridViewModel
    {
        #region Fields

        private ObservableCollection<DataGridItem> articles;

        private DataGridItem selectedArticle;

        #endregion

        #region Properties

        public ObservableCollection<DataGridItem> Articles { get => this.articles; set => this.SetProperty(ref this.articles, value); }

        public BindableBase NavigationViewModel { get; set; }

        public DataGridItem SelectedArticle { get => this.selectedArticle; set => this.SetProperty(ref this.selectedArticle, value); }

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
