using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : DetailsViewModel<ItemDetails>, IExtensionDataEntityViewModel, IEdit
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IEnumerable<Compartment> compartmentsDataSource;

        private ICommand deleteCommand;

        private bool itemHasCompartments;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private Compartment selectedCompartment;

        private ICommand withdrawCommand;

        #endregion

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public IEnumerable<Compartment> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteCommandAsync()));

        public bool ItemHasCompartments
        {
            get => this.itemHasCompartments;
            set => this.SetProperty(ref this.itemHasCompartments, value);
        }

        public Compartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public ICommand WithdrawCommand => this.withdrawCommand ??
                                          (this.withdrawCommand = new DelegateCommand(
                                               this.ExecuteWithdraw,
                                               this.CanExecuteWithdraw));

        #endregion

        #region Methods

        public override async void LoadRelatedData()
        {
            this.CompartmentsDataSource = this.Model != null
                ? await this.compartmentProvider.GetByItemIdAsync(this.Model.Id)
                : null;
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.WithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.itemProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Item, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<Item>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<Item, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Item>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanExecuteWithdraw()
        {
            return this.Model?.TotalAvailable > 0;
        }

        private async Task DeleteItemAsync()
        {
            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, BusinessObjects.ItemListRow),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var result = await this.itemProvider.DeleteAsync(this.Model.Id);
                if (result.Success)
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
                    this.EventService.Invoke(new RefreshModelsPubSubEvent<Item>(this.Model.Id));
                    this.HistoryViewService.Previous();
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }
        }

        private async Task ExecuteDeleteCommandAsync()
        {
            var deleteAction = await this.itemProvider.CanDeleteAsync(this.Model.Id);
            if (deleteAction.IsAllowed)
            {
                await this.DeleteItemAsync();
            }
            else
            {
                this.ShowErrorDialog(deleteAction);
            }
        }

        private void ExecuteWithdraw()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.WITHDRAWDIALOG,
                new
                {
                    Id = this.Model.Id
                });

            this.IsBusy = false;
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<Item>>(async eventArgs => await this.LoadDataAsync(), this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<Item, int>>(async eventArgs => await this.LoadDataAsync());
            this.modelSelectionChangedSubscription = this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<Item>>(
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
                true,
                true);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    this.Model = await this.itemProvider.GetByIdAsync(modelId);
                    this.ItemHasCompartments = this.Model.CompartmentsCount > 0 ? true : false;
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }
        }

        #endregion
    }
}
