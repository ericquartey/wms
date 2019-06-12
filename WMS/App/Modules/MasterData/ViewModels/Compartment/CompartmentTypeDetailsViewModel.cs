using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentTypeDetailsViewModel : DetailsViewModel<CompartmentType>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider = ServiceLocator.Current.GetInstance<ICompartmentTypeProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IEnumerable<AssociateItemWithCompartmentType> associatedItemsDataSource;

        private bool hasAssociatedItem;

        private AssociateItemWithCompartmentType selectedAssociatedItem;

        #endregion

        #region Properties

        public IEnumerable<AssociateItemWithCompartmentType> AssociatedItemsDataSource
        {
            get => this.associatedItemsDataSource;
            set => this.SetProperty(ref this.associatedItemsDataSource, value);
        }

        public bool HasAssociatedItem
        {
            get => this.hasAssociatedItem;
            set => this.SetProperty(ref this.hasAssociatedItem, value);
        }

        public AssociateItemWithCompartmentType SelectedAssociatedItem
        {
            get => this.selectedAssociatedItem;
            set => this.SetProperty(ref this.selectedAssociatedItem, value);
        }

        #endregion

        #region Methods

        protected override async Task<bool> ExecuteDeleteCommandAsync()
        {
            var result = await this.compartmentTypeProvider.DeleteAsync(this.Model.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.CompartmentTypeDeletedSuccesfully, StatusType.Success));
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
            throw new System.NotImplementedException();
        }

        protected override async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;

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
                        this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
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

        #endregion
    }
}
