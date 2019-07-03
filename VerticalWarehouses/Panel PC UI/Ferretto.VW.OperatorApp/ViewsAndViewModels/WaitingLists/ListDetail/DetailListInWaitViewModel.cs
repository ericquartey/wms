using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists.ListDetail
{
    public class DetailListInWaitViewModel : BindableBase, IDetailListInWaitViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlListDetailDataGridViewModel dataGridViewModelRef;

        private IEventAggregator eventAggregator;

        private ObservableCollection<DataGridListDetail> lists;

        #endregion

        #region Constructors

        public DetailListInWaitViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlListDetailDataGridViewModel>() as CustomControlListDetailDataGridViewModel;
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.lists = new ObservableCollection<DataGridListDetail>();
            for (int i = 0; i < random.Next(0, 30); i++)
            {
                this.lists.Add(new DataGridListDetail
                {
                    Item = $"Item {i}",
                    Description = $"This is item {i}",
                    Machine = $"{random.Next(0, 30)}",
                    Quantity = $"{random.Next(10, 1000)}",
                    Row = $"{random.Next(0, 20)}",
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
