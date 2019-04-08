using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : DetailsViewModel<ItemDetails>, IEdit
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IEnumerable<Compartment> compartmentsDataSource;

        private bool itemHasCompartments;

        private object modelSelectionChangedSubscription;

        private Compartment selectedCompartment;

        private ICommand withdrawItemCommand;

        private string withdrawReason;

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

        public ICommand WithdrawItemCommand => this.withdrawItemCommand ??
            (this.withdrawItemCommand = new DelegateCommand(
                this.WithdrawItem));

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override async void LoadRelatedData()
        {
            if (!this.IsModelIdValid)
            {
                return;
            }

            this.CompartmentsDataSource = this.Model != null
                ? await this.compartmentProvider.GetByItemIdAsync(this.Model.Id)
                : null;
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.WithdrawReason = this.Model?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.WithdrawItemCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var result = await this.itemProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
                this.EventService.Invoke(new RefreshModelsPubSubEvent<Item>(this.Model.Id));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
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
            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            this.IsBusy = true;

            var result = await this.itemProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

                if (this.Data is int modelId)
                {
                    this.Model = await this.itemProvider.GetByIdAsync(modelId);
                    this.ItemHasCompartments = this.Model.CompartmentsCount > 0;
                }

                this.IsBusy = false;
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
            }
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<Item>>(this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private void Initialize()
        {
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

        private void WithdrawItem()
        {
            if (!this.Model.CanExecuteOperation(nameof(BusinessPolicies.Withdraw)))
            {
                this.ShowErrorDialog(this.Model.GetCanExecuteOperationReason(nameof(BusinessPolicies.Withdraw)));
                return;
            }

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

        #endregion
    }
}
