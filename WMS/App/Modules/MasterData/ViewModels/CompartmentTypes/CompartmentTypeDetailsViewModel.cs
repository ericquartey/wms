using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Interfaces;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.CompartmentType))]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemCompartmentType), false)]
    public class CompartmentTypeDetailsViewModel : DetailsViewModel<CompartmentType>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider = ServiceLocator.Current.GetInstance<ICompartmentTypeProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IEnumerable<ItemWithCompartmentTypeInfo> associatedItemsDataSource;

        private ICommand associateItemCommand;

        private CompartmentTypeInput compartmentTypeInput;

        private ICommand deleteAssociationCommand;

        private string deleteAssociationReason;

        private bool hasAssociatedItem;

        private bool isAddAssociateItemShown;

        private IEnumerable<Item> itemsDataSource;

        private ICommand openCreateNewAssociationCommand;

        private ItemWithCompartmentTypeInfo selectedAssociatedItem;

        #endregion

        #region Properties

        public IEnumerable<ItemWithCompartmentTypeInfo> AssociatedItemsDataSource
        {
            get => this.associatedItemsDataSource;
            set => this.SetProperty(ref this.associatedItemsDataSource, value);
        }

        public ICommand AssociateItemCommand => this.associateItemCommand ??
                            (this.associateItemCommand = new DelegateCommand(
                        async () => await this.AssociateItemAsync()));

        public CompartmentTypeInput CompartmentTypeInput
        {
            get => this.compartmentTypeInput;
            set => this.SetProperty(ref this.compartmentTypeInput, value);
        }

        public ICommand DeleteAssociationCommand => this.deleteAssociationCommand ??
                            (this.deleteAssociationCommand = new DelegateCommand(
                async () => await this.DeleteAssociationAsync(),
                this.CanDeleteAssociation));

        public string DeleteAssociationReason
        {
            get => this.deleteAssociationReason;
            set => this.SetProperty(ref this.deleteAssociationReason, value);
        }

        public bool HasAssociatedItem
        {
            get => this.hasAssociatedItem;
            set => this.SetProperty(ref this.hasAssociatedItem, value);
        }

        public bool IsAddAssociateItemShown
        {
            get => this.isAddAssociateItemShown;
            set => this.SetProperty(ref this.isAddAssociateItemShown, value);
        }

        [Display(Name = nameof(BusinessObjects.ItemAvailable), ResourceType = typeof(BusinessObjects))]
        public IEnumerable<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public ICommand OpenCreateNewAssociationCommand => this.openCreateNewAssociationCommand ??
                                                 (this.openCreateNewAssociationCommand = new DelegateCommand(
                 this.OpenCreateNewAssociation));

        public ItemWithCompartmentTypeInfo SelectedAssociatedItem
        {
            get => this.selectedAssociatedItem;
            set => this.SetProperty(ref this.selectedAssociatedItem, value);
        }

        #endregion

        #region Methods

        public bool CanDeleteAssociation()
        {
            return this.SelectedAssociatedItem != null;
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.DeleteAssociationReason = this.SelectedAssociatedItem?.GetCanDeleteReason();
        }

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var result = await this.compartmentTypeProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.CompartmentTypeDeletedSuccesfully, StatusType.Success));
                this.OnDispose();
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

        protected override Task ExecuteRevertCommandAsync()
        {
            throw new System.NotSupportedException();
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.ItemsDataSource = null;

                if (this.Data is int modelId)
                {
                    this.Model = await this.compartmentTypeProvider.GetByIdAsync(modelId);

                    var result = await this.itemProvider.GetAllAssociatedByCompartmentTypeIdAsync(this.Model.Id);
                    if (result.Success)
                    {
                        this.AssociatedItemsDataSource = result.Entity;
                    }
                    else
                    {
                        this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
                    }

                    var resultAllowed = await this.itemProvider.GetAllAllowedByCompartmentTypeIdAsync(this.Model.Id);
                    if (resultAllowed.Success)
                    {
                        this.ItemsDataSource = resultAllowed.Entity;
                    }
                }

                this.HasAssociatedItem = this.AssociatedItemsDataSource != null ? this.AssociatedItemsDataSource.Any() : false;

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

        private async Task<bool> AssociateItemAsync()
        {
            this.CompartmentTypeInput.IsValidationEnabled = true;

            if (!string.IsNullOrEmpty(this.CompartmentTypeInput.Error))
            {
                return false;
            }

            this.IsBusy = true;
            this.CompartmentTypeInput.IsValidationEnabled = false;

            var resultCreate = await this.compartmentTypeProvider.CreateAsync(
                this.Model,
                this.CompartmentTypeInput.ItemId,
                (int?)this.CompartmentTypeInput.MaxCapacity);

            if (resultCreate.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.AssociationCompartmentTypeCreatedSuccessfully, StatusType.Success));
                this.IsAddAssociateItemShown = false;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return resultCreate.Success;
        }

        private async Task DeleteAssociationAsync()
        {
            if (!this.selectedAssociatedItem.CanDelete())
            {
                this.ShowErrorDialog(this.selectedAssociatedItem.GetCanDeleteReason());
                return;
            }

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, string.Empty),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var success = await this.ExecuteDeleteAssociationAsync();
                if (success)
                {
                    await this.LoadDataAsync();
                }
            }
        }

        private async Task<bool> ExecuteDeleteAssociationAsync()
        {
            this.IsBusy = true;
            var resultDelete = await this.compartmentTypeProvider.DeleteAssociationAsync(
                this.Model.Id,
                this.SelectedAssociatedItem.Id);

            if (resultDelete.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.AssociationCompartmentTypeDeletedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return resultDelete.Success;
        }

        private void OpenCreateNewAssociation()
        {
            this.CompartmentTypeInput = new CompartmentTypeInput();
        }

        #endregion
    }
}
