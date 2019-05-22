using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
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

        private ICommand pickItemCommand;

        private string pickReason;

        private ICommand putItemCommand;

        private string putReason;

        private Compartment selectedCompartment;

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

        public ICommand PickItemCommand => this.pickItemCommand ??
            (this.pickItemCommand = new DelegateCommand(
             this.PickItem));

        public string PickReason
        {
            get => this.pickReason;
            set => this.SetProperty(ref this.pickReason, value);
        }

        public ICommand PutItemCommand => this.putItemCommand ??
                    (this.putItemCommand = new DelegateCommand(
             this.PutItem));

        public string PutReason
        {
            get => this.putReason;
            set => this.SetProperty(ref this.putReason, value);
        }

        public Compartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
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
            this.PickReason = this.Model?.GetCanExecuteOperationReason(nameof(ItemPolicy.Pick));
            this.PutReason = this.Model?.GetCanExecuteOperationReason(nameof(ItemPolicy.Put));
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.PickItemCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task<bool> ExecuteCompleteCommandAsync()
        {
            this.IsBusy = true;

            var result = await this.itemProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return result.Success;
        }

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var result = await this.itemProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
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

            this.IsBusy = true;

            if (!await base.ExecuteSaveCommandAsync())
            {
                return false;
            }

            var result = await this.itemProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return result.Success;
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

        private void PickItem()
        {
            if (!this.Model.CanExecuteOperation(nameof(ItemPolicy.Pick)))
            {
                this.ShowErrorDialog(this.Model.GetCanExecuteOperationReason(nameof(ItemPolicy.Pick)));
                return;
            }

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMPICK,
                new
                {
                    Id = this.Model.Id
                });
        }

        private void PutItem()
        {
            if (!this.Model.CanExecuteOperation(nameof(ItemPolicy.Put)))
            {
                this.ShowErrorDialog(this.Model.GetCanExecuteOperationReason(nameof(ItemPolicy.Put)));
                return;
            }

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMPUT,
                new
                {
                    Id = this.Model.Id
                });
        }

        #endregion
    }
}
