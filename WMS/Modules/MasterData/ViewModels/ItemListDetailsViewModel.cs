using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Layout.Core;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListDetailsViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService
                    dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();

        private readonly IItemListProvider itemListProvider =
                    ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ICommand addListRowCommand;
        private ICommand editListRowCommand;
        private ItemListDetails itemList;
        private IEnumerable<ItemListRow> itemListRowDataSource;
        private ICommand listExecuteCommand;
        private ICommand listRowExecuteCommand;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object modelSelectionChangedSubscription;
        private ICommand revertCommand;
        private ICommand saveCommand;
        private ItemListRow selectedItemListRow;

        #endregion Fields

        #region Constructors

        public ItemListDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public ICommand AddListRowCommand => this.addListRowCommand ??
                                   (this.addListRowCommand = new DelegateCommand(this.ExecuteAddListRowCommand,
                       this.CanExecuteAddListRowCommand)
             .ObservesProperty(() => this.SelectedItemListRow));

        public ICommand EditListRowCommand => this.editListRowCommand ??
                                   (this.editListRowCommand = new DelegateCommand(this.ExecuteEditListRowCommand,
                       this.CanExecuteEditListRowCommand)
             .ObservesProperty(() => this.SelectedItemListRow));

        public ItemListDetails ItemList
        {
            get => this.itemList;
            set
            {
                if (this.ItemList != null && value != this.ItemList)
                {
                    this.ItemList.TakeSnapshot();
                    this.ItemList.PropertyChanged -= this.OnItemListPropertyChanged;
                }

                if (!this.SetProperty(ref this.itemList, value))
                {
                    return;
                }

                this.ItemList.TakeSnapshot();
                this.ItemList.PropertyChanged += this.OnItemListPropertyChanged;

                //TODO
                //this.RefreshData();
            }
        }

        public IEnumerable<ItemListRow> ItemListRowDataSource
        {
            get => this.itemListRowDataSource;
            set => this.SetProperty(ref this.itemListRowDataSource, value);
        }

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                                   (this.listExecuteCommand = new DelegateCommand(this.ExecuteListCommand,
                       this.CanExecuteListCommand)
             .ObservesProperty(() => this.ItemList));

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(this.ExecuteListRowCommand,
                       this.CanExecuteListRowCommand)
             .ObservesProperty(() => this.SelectedItemListRow));

        public ICommand RevertCommand => this.revertCommand ??
                                          (this.revertCommand = new DelegateCommand(this.LoadData, this.CanExecuteRevert));

        public ICommand SaveCommand => this.saveCommand ??
                  (this.saveCommand = new DelegateCommand(this.ExecuteSaveCommand, this.CanExecuteSave));

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

        private Boolean CanExecuteAddListRowCommand()
        {
            return this.selectedItemListRow != null;
        }

        private Boolean CanExecuteEditListRowCommand()
        {
            return this.selectedItemListRow != null;
        }

        private bool CanExecuteListCommand()
        {
            if (this.ItemList != null)
            {
                var status = this.ItemList.ItemListStatus;
                if (status == ItemListStatus.Incomplete
                    || status == ItemListStatus.Suspended
                    || status == ItemListStatus.Waiting)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanExecuteListRowCommand()
        {
            if (this.selectedItemListRow != null)
            {
                var status = this.selectedItemListRow.ItemListRowStatus;
                if (status == ItemListRowStatus.Incomplete
                    || status == ItemListRowStatus.Suspended
                    || status == ItemListRowStatus.Waiting)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanExecuteRevert()
        {
            return this.ItemList?.IsModified == true;
        }

        private bool CanExecuteSave()
        {
            return this.ItemList?.IsModified == true;
        }

        private void ExecuteAddListRowCommand()
        {
            //TODO
        }

        private void ExecuteEditListRowCommand()
        {
            //TODO
        }

        private void ExecuteListCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTDIALOG,
                new
                {
                    Id = this.SelectedItemListRow.Id
                }
            );
        }

        private void ExecuteListRowCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTROWDIALOG,
                new
                {
                    Id = this.ItemList.Id
                }
            );
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemListProvider.Save(this.ItemList);

            if (modifiedRowCount > 0)
            {
                this.ItemList.TakeSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<Item>(this.ItemList.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemListSavedSuccessfully));
            }
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

        private void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(this.ItemList.IsModified))
            {
                ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
                ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
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
