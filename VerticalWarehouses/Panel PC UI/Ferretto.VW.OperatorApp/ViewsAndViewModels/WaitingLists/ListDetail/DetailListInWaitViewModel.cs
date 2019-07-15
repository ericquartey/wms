using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.WmsCommunication.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail
{
    public class DetailListInWaitViewModel : BindableBase, IDetailListInWaitViewModel
    {
        #region Fields

        private readonly CustomControlListDetailDataGridViewModel dataGridViewModelRef;

        private readonly IEventAggregator eventAggregator;

        private readonly IWmsDataProvider wmsDataProvider;

        private BindableBase dataGridViewModel;

        private DataGridList list;

        private ObservableCollection<DataGridListDetail> lists;

        #endregion

        #region Constructors

        public DetailListInWaitViewModel(
            IEventAggregator eventAggregator,
            ICustomControlListDetailDataGridViewModel listDetailDataGridViewModel,
            IWmsDataProvider wmsDataProvider)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
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
            set
            {
                this.list = value;
            }
        }

        public ICustomControlListDetailDataGridViewModel ListDetailDataGridViewModel { get; }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            this.lists = new ObservableCollection<DataGridListDetail>();

            var tmpLists = new ObservableCollection<WMS.Data.WebAPI.Contracts.ItemListRow>();

            try
            {
                tmpLists = await this.wmsDataProvider.GetListRowsAsync(this.List.List);
            }
            catch (Exception ex)
            {
                throw new Exception("DetailList - " + ex.Message);
            }

            for (var i = 0; i < tmpLists.Count; i++)
            {
                string machines = string.Empty;
                for (int j = 0; j < tmpLists[i].Machines.Count; j++)
                {
                    machines = (j == tmpLists[i].Machines.Count - 1) ?
                        string.Concat(machines, tmpLists[i].Machines[j].Id.ToString()) : string.Concat(machines, tmpLists[i].Machines[j].Id.ToString(), ", ");
                }

                if (string.IsNullOrEmpty(machines))
                {
                    machines = "---";
                }

                this.lists.Add(new DataGridListDetail
                {
                    Item = $"{tmpLists[i].ItemListId}",
                    Description = $"{tmpLists[i].ItemDescription}",
                    Machine = machines,
                    Quantity = $"{tmpLists[i].RequestedQuantity}",
                    Row = $"{tmpLists[i].Id}",
                }
                );
            }
            this.dataGridViewModelRef.Lists = this.lists;
            this.dataGridViewModelRef.SelectedList = this.lists[0];
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
