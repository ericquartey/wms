using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.ItemLists
{
    public class ItemListDetailsViewModel : DetailsViewModel<ItemListDetails>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private readonly IItemListRowProvider itemListRowProvider =
            ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private ICommand addListRowCommand;

        private string addRowReason;

        private ICommand deleteListRowCommand;

        private string deleteRowReason;

        private ICommand executeListCommand;

        private ICommand executeListRowCommand;

        private string executeReason;

        private string executeRowReason;

        private bool listHasRows;

        private object modelSelectionChangedSubscription;

        private ItemListRow selectedItemListRow;

        private ICommand showListRowDetailsCommand;

        #endregion

        #region Constructors

        public ItemListDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand AddListRowCommand => this.addListRowCommand ??
            (this.addListRowCommand = new DelegateCommand(
                this.AddListRow,
                this.CanAddListRow));

        public string AddRowReason
        {
            get => this.addRowReason;
            set => this.SetProperty(ref this.addRowReason, value);
        }

        public ICommand DeleteListRowCommand => this.deleteListRowCommand ??
                    (this.deleteListRowCommand = new DelegateCommand(
                async () => await this.DeleteListRowAsync(),
                this.CanDeleteListRow).ObservesProperty(() => this.SelectedItemListRow));

        public string DeleteRowReason
        {
            get => this.deleteRowReason;
            set => this.SetProperty(ref this.deleteRowReason, value);
        }

        public ICommand ExecuteListCommand => this.executeListCommand ??
                    (this.executeListCommand = new DelegateCommand(
                this.ExecuteList));

        public ICommand ExecuteListRowCommand => this.executeListRowCommand ??
            (this.executeListRowCommand = new DelegateCommand(
                    this.ExecuteListRow,
                    this.CanExecuteListRow)
                .ObservesProperty(() => this.SelectedItemListRow));

        public string ExecuteReason
        {
            get => this.executeReason;
            set => this.SetProperty(ref this.executeReason, value);
        }

        public string ExecuteRowReason
        {
            get => this.executeRowReason;
            set => this.SetProperty(ref this.executeRowReason, value);
        }

        public bool ListHasRows
        {
            get => this.listHasRows;
            set => this.SetProperty(ref this.listHasRows, value);
        }

        public ItemListRow SelectedItemListRow
        {
            get => this.selectedItemListRow;
            set => this.SetProperty(ref this.selectedItemListRow, value);
        }

        public ICommand ShowListRowDetailsCommand => this.showListRowDetailsCommand ??
                                            (this.showListRowDetailsCommand = new DelegateCommand(
                this.ShowListRowDetails,
                this.CanShowListRowDetails).ObservesProperty(() => this.SelectedItemListRow));

        #endregion

        #region Methods

        public override void LoadRelatedData()
        {
            // TODO: implement method
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.ExecuteReason = this.Model?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Execute))
                .Select(p => p.Reason).FirstOrDefault();
            this.ExecuteRowReason = this.SelectedItemListRow?.Policies
                ?.Where(p => p.Name == nameof(BusinessPolicies.Execute)).Select(p => p.Reason).FirstOrDefault();
            this.AddRowReason = this.SelectedItemListRow?.Policies?.Where(p => p.Name == nameof(CommonPolicies.Create))
                .Select(p => p.Reason).FirstOrDefault();
            this.DeleteRowReason = this.SelectedItemListRow?.Policies
                ?.Where(p => p.Name == nameof(CommonPolicies.Delete)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.ExecuteListCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ExecuteListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ShowListRowDetailsCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.AddListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.DeleteListRowCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var result = await this.itemListProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.ItemLists.ItemListDeletedSuccessfully,
                    StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            return result.Success;
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task<bool> ExecuteSaveCommandAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            this.IsBusy = true;

            var result = await this.itemListProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.ItemLists.ItemListSavedSuccessfully,
                    StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.Errors.UnableToSaveChanges,
                    StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override async Task LoadDataAsync()
        {
            if (this.Data is int modelId)
            {
                try
                {
                    this.IsBusy = true;

                    this.Model = await this.itemListProvider.GetByIdAsync(modelId);
                    this.ListHasRows = this.Model.ItemListRowsCount > 0;
                    this.IsBusy = false;
                }
                catch
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
                }
            }
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<ItemList>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void AddListRow()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                nameof(ItemLists),
                Common.Utils.Modules.ItemLists.ITEMLISTROWADD,
                this.Model.Id);

            this.IsBusy = false;
        }

        private bool CanAddListRow()
        {
            return this.Model?.CanExecuteOperation("AddRow") == true;
        }

        private bool CanDeleteListRow()
        {
            return this.selectedItemListRow != null;
        }

        private bool CanExecuteListRow()
        {
            return this.selectedItemListRow != null;
        }

        private bool CanShowListRowDetails()
        {
            return this.selectedItemListRow != null;
        }

        private async Task DeleteListRowAsync()
        {
            if (this.SelectedItemListRow.CanDelete())
            {
                this.IsBusy = true;

                var userChoice = this.DialogService.ShowMessage(
                    string.Format(DesktopApp.AreYouSureToDeleteGeneric, BusinessObjects.ItemListRow),
                    DesktopApp.ConfirmOperation,
                    DialogType.Question,
                    DialogButtons.YesNo);

                if (userChoice == DialogResult.Yes)
                {
                    var result = await this.itemListRowProvider.DeleteAsync(this.SelectedItemListRow.Id);
                    if (result.Success)
                    {
                        this.EventService.Invoke(
                            new StatusPubSubEvent(
                                Common.Resources.ItemLists.ItemListRowDeletedSuccessfully,
                                StatusType.Success));
                        this.IsBusy = false;
                        this.SelectedItemListRow = null;

                        await this.LoadDataAsync();
                    }
                    else
                    {
                        this.EventService.Invoke(new StatusPubSubEvent(
                            Common.Resources.Errors.UnableToSaveChanges,
                            StatusType.Error));
                    }
                }

                this.IsBusy = false;
            }
            else
            {
                this.ShowErrorDialog(this.SelectedItemListRow.GetCanDeleteReason());
            }
        }

        private void ExecuteList()
        {
            if (this.Model?.CanExecuteOperation(BusinessPolicies.Execute.ToString()) == true)
            {
                this.IsBusy = true;

                this.NavigationService.Appear(
                    nameof(ItemLists),
                    Common.Utils.Modules.ItemLists.EXECUTELIST,
                    new
                    {
                        Id = this.Model.Id
                    });

                this.IsBusy = false;
            }
            else
            {
                this.ShowErrorDialog(this.Model.GetCanExecuteOperationReason(BusinessPolicies.Execute.ToString()));
            }
        }

        private void ExecuteListRow()
        {
            if (this.selectedItemListRow.CanExecuteOperation(BusinessPolicies.Execute.ToString()) == true)
            {
                this.IsBusy = true;

                this.NavigationService.Appear(
                    nameof(ItemLists),
                    Common.Utils.Modules.ItemLists.EXECUTELISTROW,
                    new
                    {
                        Id = this.SelectedItemListRow.Id
                    });

                this.IsBusy = false;
            }
            else
            {
                this.ShowErrorDialog(this.selectedItemListRow.GetCanExecuteOperationReason(
                    BusinessPolicies.Execute.ToString()));
            }
        }

        private void Initialize()
        {
            this.modelSelectionChangedSubscription = this.EventService
                .Subscribe<ModelSelectionChangedPubSubEvent<ItemList>>(
                    async eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            await this.LoadDataAsync();
                        }
                        else
                        {
                            this.Model = null;
                        }
                    },
                    this.Token,
                    keepSubscriberReferenceAlive: true,
                    forceUiThread: true);
        }

        private void ShowListRowDetails()
        {
            this.HistoryViewService.Appear(
                nameof(ItemLists),
                Common.Utils.Modules.ItemLists.ITEMLISTROWDETAILS,
                this.SelectedItemListRow.Id);
        }

        #endregion
    }
}
