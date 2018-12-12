using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
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

        private readonly ChangeDetector<ItemListDetails> changeDetector = new ChangeDetector<ItemListDetails>();
        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();
        private ICommand addListRowCommand;
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
        private ICommand showDetailsListRowCommand;

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

        public ItemListDetails ItemList
        {
            get => this.itemList;
            set
            {
                if (this.itemList == value)
                {
                    return;
                }

                if (this.itemList != null)
                {
                    this.ItemList.PropertyChanged -= this.OnItemListPropertyChanged;
                }

                this.SetProperty(ref this.itemList, value);

                this.changeDetector.TakeSnapshot(this.ItemList);

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

        public ICommand ShowDetailsListRowCommand => this.showDetailsListRowCommand ??
                  (this.showDetailsListRowCommand = new DelegateCommand(this.ExecuteShowDetailsListRowCommand,
                       this.CanExecuteShowDetailsListRowCommand)
             .ObservesProperty(() => this.SelectedItemListRow));

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
            return this.changeDetector.IsModified == true;
        }

        private bool CanExecuteSave()
        {
            return this.changeDetector.IsModified == true;
        }

        private bool CanExecuteShowDetailsListRowCommand()
        {
            return this.selectedItemListRow != null;
        }

        private void ChangeDetector_ModifiedChanged(System.Object sender, System.EventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        private void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RevertCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.SaveCommand)?.RaiseCanExecuteChanged();
        }

        private void ExecuteAddListRowCommand()
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
                    Id = this.ItemList.Id
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
                    Id = this.SelectedItemListRow.Id
                }
            );
        }

        private void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemListProvider.Save(this.ItemList);
            if (modifiedRowCount > 0)
            {
                this.changeDetector.TakeSnapshot(this.ItemList);

                this.EventService.Invoke(new ModelChangedEvent<Item>(this.ItemList.Id));

                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemListSavedSuccessfully));
            }
        }

        private void ExecuteShowDetailsListRowCommand()
        {
            //TODO
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

            this.changeDetector.ModifiedChanged += this.ChangeDetector_ModifiedChanged;
        }

        private void LoadData()
        {
            if ((this.Data is int modelId))
            {
                this.ItemList = this.itemListProvider.GetById(modelId);
            }
        }

        private void OnItemListPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.EvaluateCanExecuteCommands();
        }

        #endregion Methods
    }
}
