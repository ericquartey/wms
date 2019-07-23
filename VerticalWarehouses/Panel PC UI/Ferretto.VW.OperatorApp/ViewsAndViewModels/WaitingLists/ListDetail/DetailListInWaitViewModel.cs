using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail
{
    public class DetailListInWaitViewModel : BaseViewModel, IDetailListInWaitViewModel
    {
        #region Fields

        private readonly CustomControlListDetailDataGridViewModel dataGridViewModelRef;

        private readonly IWmsDataProvider wmsDataProvider;

        private BindableBase dataGridViewModel;

        private DataGridList list;

        private IEnumerable<DataGridListDetail> lists;

        #endregion

        #region Constructors

        public DetailListInWaitViewModel(
            ICustomControlListDetailDataGridViewModel listDetailDataGridViewModel,
            IWmsDataProvider wmsDataProvider)
        {
            if (listDetailDataGridViewModel == null)
            {
                throw new ArgumentNullException(nameof(listDetailDataGridViewModel));
            }

            if (wmsDataProvider == null)
            {
                throw new ArgumentNullException(nameof(wmsDataProvider));
            }

            this.ListDetailDataGridViewModel = listDetailDataGridViewModel;
            this.wmsDataProvider = wmsDataProvider;
            this.dataGridViewModelRef = listDetailDataGridViewModel as CustomControlListDetailDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public DataGridList List
        {
            get => this.list;
            set => this.list = value;
        }

        public ICustomControlListDetailDataGridViewModel ListDetailDataGridViewModel { get; }

        #endregion

        #region Methods

        public override async Task OnEnterViewAsync()
        {
            try
            {
                var listRows = await this.wmsDataProvider.GetListRowsAsync(this.List.Id);

                this.lists = listRows.Select(r =>
                    new DataGridListDetail
                    {
                        Machine = string.Join(",", r.Machines.Select(m => m.Nickname)) ?? "-",
                        Item = r.ItemCode,
                        Description = r.ItemDescription,
                        Quantity = r.RequestedQuantity.ToString(),
                        Row = r.Code
                    });

                this.dataGridViewModelRef.Lists = this.lists;
                this.dataGridViewModelRef.SelectedList = this.lists.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception("DetailList - " + ex.Message);
            }
        }

        #endregion
    }
}
