using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Layout.Core;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService
                    dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();

        private readonly IItemListProvider itemListProvider =
                    ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ItemListDetails itemList;

        private IEnumerable<ItemListRow> itemListRowDataSource;
        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;
        private ItemListRow selectedItemListRow;

        #endregion Fields

        #region Constructors

        public ItemListDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ItemListDetails ItemList
        {
            get => this.itemList;
            set => this.SetProperty(ref this.itemList, value);
        }

        public IEnumerable<ItemListRow> ItemListRowDataSource
        {
            get => this.itemListRowDataSource;
            set => this.SetProperty(ref this.itemListRowDataSource, value);
        }

        public ItemListRow SelectedItemListRow
        {
            get => this.selectedItemListRow;
            set => this.SetProperty(ref this.selectedItemListRow, value);
        }

        public string StatusColor { get; private set; }

        #endregion Properties

        #region Methods

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        private void Initialize()
        {
            this.LoadData();
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<ItemList>>(eventArgs => { this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<ItemList>>(eventArgs => { this.LoadData(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedEvent<ItemList>>(
                    eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            this.LoadData();
                        }
                        else
                        {
                            this.ItemList = null;
                        }
                    },
                    this.Token,
                    true,
                    true);
        }

        private void LoadData()
        {
            if ((this.Data is int modelId))
            {
                this.ItemList = this.itemListProvider.GetById(modelId);
                this.SetColorStatus();
            }
        }

        private void SetColorStatus()
        {
            if (this.ItemList != null && this.ItemList.ItemListStatus > 0)
            {
                switch (this.ItemList.ItemListStatus)
                {
                    case ItemListStatus.Waiting:
                        this.StatusColor = Application.Current.Resources["WaitingStatus"].ToString();//cyan
                        break;

                    case ItemListStatus.Executing:
                        this.StatusColor = Application.Current.Resources["ExecutingStatus"].ToString();//blue
                        break;

                    case ItemListStatus.Completed:
                        this.StatusColor = Application.Current.Resources["CompletedStatus"].ToString();//green
                        break;

                    case ItemListStatus.Incomplete:
                        this.StatusColor = Application.Current.Resources["IncompleteStatus"].ToString();//red
                        break;

                    case ItemListStatus.Suspended:
                        this.StatusColor = Application.Current.Resources["SuspendedStatus"].ToString();//orange
                        break;
                }
            }
        }

        #endregion Methods
    }
}
