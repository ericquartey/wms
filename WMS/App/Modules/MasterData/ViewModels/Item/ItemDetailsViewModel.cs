using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemDetailsViewModel : DetailsViewModel<ItemDetails>, IEdit
    {
        #region Fields

        private readonly IAreaProvider areaProvider = ServiceLocator.Current.GetInstance<IAreaProvider>();

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IEnumerable<AllowedItemArea> allowedItemAreasDataSource;

        private int? areaId;

        private ICommand associateAreaCommand;

        private string associateAreaReason;

        private ICommand associateCompartmentTypeCommand;

        private IEnumerable<Area> availableAreasDataSource;

        private IEnumerable<Compartment> compartmentsDataSource;

        private bool isAddAreaShown;

        private bool itemHasCompartments;

        private object modelSelectionChangedSubscription;

        private ICommand pickItemCommand;

        private string pickReason;

        private ICommand putItemCommand;

        private string putReason;

        private AllowedItemArea selectedAllowedItemArea;

        private Compartment selectedCompartment;

        private ICommand showAssociateAreaCommand;

        private ICommand unassociateAreaCommand;

        private string unassociateAreaReason;

        #endregion

        #region Constructors

        public ItemDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public IEnumerable<AllowedItemArea> AllowedItemAreasDataSource
        {
            get => this.allowedItemAreasDataSource;
            set => this.SetProperty(ref this.allowedItemAreasDataSource, value);
        }

        public int? AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public ICommand AssociateAreaCommand => this.associateAreaCommand ??
                    (this.associateAreaCommand = new DelegateCommand(
                        async () => await this.AssociateAreaAsync(),
                        this.CanAssociateArea)
                        .ObservesProperty(() => this.AreaId));

        public string AssociateAreaReason
        {
            get => this.associateAreaReason;
            set => this.SetProperty(ref this.associateAreaReason, value);
        }

        public ICommand AssociateCompartmentTypeCommand => this.associateCompartmentTypeCommand ??
            (this.associateCompartmentTypeCommand = new DelegateCommand(
             this.AssociateCompartmentType));

        public IEnumerable<Area> AvailableAreasDataSource
        {
            get => this.availableAreasDataSource;
            set => this.SetProperty(ref this.availableAreasDataSource, value);
        }

        public IEnumerable<Compartment> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public bool IsAddAreaShown
        {
            get => this.isAddAreaShown;
            set => this.SetProperty(ref this.isAddAreaShown, value);
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

        public AllowedItemArea SelectedAllowedItemArea
        {
            get => this.selectedAllowedItemArea;
            set
            {
                if (this.SetProperty(ref this.selectedAllowedItemArea, value))
                {
                    this.UnassociateAreaReason = this.selectedAllowedItemArea?.GetCanDeleteReason();
                }
            }
        }

        public Compartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public ICommand ShowAssociateAreaCommand => this.showAssociateAreaCommand ??
          (this.showAssociateAreaCommand = new DelegateCommand(
               this.CheckAddArea));

        public ICommand UnassociateAreaCommand => this.unassociateAreaCommand ??
            (this.unassociateAreaCommand = new DelegateCommand(
                async () => await this.ExecuteUnassociateAreaWithPromptAsync(),
                this.CanUnassociateArea).ObservesProperty(() => this.SelectedAllowedItemArea));

        public string UnassociateAreaReason
        {
            get => this.unassociateAreaReason;
            set => this.SetProperty(ref this.unassociateAreaReason, value);
        }

        #endregion

        #region Methods

        public virtual bool CanAssociateArea()
        {
            return this.AreaId.HasValue;
        }

        public bool CanUnassociateArea()
        {
            return this.SelectedAllowedItemArea != null;
        }

        public override async void LoadRelatedData()
        {
            if (!this.IsModelIdValid)
            {
                return;
            }

            IEnumerable<Compartment> compartments = null;
            if (this.Model != null)
            {
                var result = await this.compartmentProvider.GetByItemIdAsync(this.Model.Id);
                compartments = result.Success ? result.Entity : null;
            }

            this.CompartmentsDataSource = compartments;
            await this.LoadItemAreasAsync();
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
            ((DelegateCommand)this.UnassociateAreaCommand)?.RaiseCanExecuteChanged();
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

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return result.Success;
        }

        protected async Task ExecuteUnassociateAreaWithPromptAsync()
        {
            if (this.selectedAllowedItemArea == null)
            {
                this.ShowErrorDialog(this.UnassociateAreaReason);
                return;
            }

            if (!this.selectedAllowedItemArea.CanDelete())
            {
                this.ShowErrorDialog(this.selectedAllowedItemArea.GetCanDeleteReason());
                return;
            }

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, string.Empty),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                await this.ExecuteDeleteItemAreaCommandAsync();
            }
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

        private async Task AssociateAreaAsync()
        {
            if (!this.AreaId.HasValue)
            {
                return;
            }

            var result = await this.areaProvider.CreateAllowedByItemIdAsync(this.AreaId.Value, this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.AreaAssociationCreatedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            await this.LoadItemAreasAsync();
            this.IsAddAreaShown = false;
        }

        private void AssociateCompartmentType()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ASSOCIATECOMPARTMENTTYPESSTEPS,
                this.Model);
        }

        private void CheckAddArea()
        {
            this.AreaId = null;
            if (string.IsNullOrEmpty(this.AssociateAreaReason) == false)
            {
                this.ShowErrorDialog(this.AssociateAreaReason);
            }
        }

        private async Task ExecuteDeleteItemAreaCommandAsync()
        {
            var result = await this.areaProvider.DeleteAllowedByItemIdAsync(this.selectedAllowedItemArea.Id, this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.AreaAssociationDeletedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            await this.LoadItemAreasAsync();
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

        private async Task LoadItemAreasAsync()
        {
            if (this.Model == null)
            {
                return;
            }

            var areas = await this.areaProvider.GetAllAsync();
            var allowedItemAreasResult = await this.areaProvider.GetAllowedByItemIdAsync(this.Model.Id);
            if (allowedItemAreasResult.Success)
            {
                this.AllowedItemAreasDataSource = allowedItemAreasResult.Entity;
                this.AvailableAreasDataSource = areas.Where(a => !this.allowedItemAreasDataSource.Any(aa => aa.Id == a.Id));
                this.AssociateAreaReason = (this.availableAreasDataSource.ToList().Count > 0) ? null : Common.Resources.MasterData.NoAvailableAreas;
            }
            else
            {
                this.AllowedItemAreasDataSource = null;
                this.AvailableAreasDataSource = null;
                this.AssociateAreaReason = Common.Resources.MasterData.NoAvailableAreas;
            }

            ((DelegateCommand)this.ShowAssociateAreaCommand).RaiseCanExecuteChanged();
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
