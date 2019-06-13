using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.Data.Hubs;

namespace Ferretto.WMS.Modules.MasterData
{
    public class CompartmentTypesToItemStepViewModel : StepViewModel
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private bool hasUnassociatedItemCompartmentTypes;

        private bool isBusy;

        private ItemCompartmentType selectedItemCompartmentType;

        private ObservableCollection<ItemCompartmentType> unassociateItemCompartmentTypesDataSource;

        #endregion

        #region Constructors

        public CompartmentTypesToItemStepViewModel(ICompartmentTypeProvider compartmentTypeProvider)
        {
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.HasUnassociatedItemCompartmentTypes = true;
        }

        #endregion

        #region Properties

        public bool HasUnassociatedItemCompartmentTypes
        {
            get => this.hasUnassociatedItemCompartmentTypes;
            set => this.SetProperty(ref this.hasUnassociatedItemCompartmentTypes, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ItemCompartmentType SelectedItemCompartmentType
        {
            get => this.selectedItemCompartmentType;
            set
            {
                if (this.selectedItemCompartmentType != null)
                {
                    this.selectedItemCompartmentType.PropertyChanged -= this.SelectedCompartmentType_PropertyChanged;
                    var maxCapacity = this.selectedItemCompartmentType.MaxCapacity;
                    if (maxCapacity.HasValue
                        && maxCapacity.Value.Equals(0))
                    {
                        this.selectedItemCompartmentType.MaxCapacity = null;
                        this.selectedItemCompartmentType.IsActive = false;
                    }
                }

                if (this.SetProperty(ref this.selectedItemCompartmentType, value) &&
                    this.selectedItemCompartmentType != null)
                {
                    this.selectedItemCompartmentType.PropertyChanged += this.SelectedCompartmentType_PropertyChanged;
                }

                this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateError));
            }
        }

        public ObservableCollection<ItemCompartmentType> UnassociateItemCompartmentTypesDataSource
        {
            get => this.unassociateItemCompartmentTypesDataSource;
            set => this.SetProperty(ref this.unassociateItemCompartmentTypesDataSource, value);
        }

        #endregion

        #region Methods

        public override bool CanGoToNextView()
        {
            return false;
        }

        public override bool CanSave()
        {
            return this.IsValid();
        }

        public override string GetError()
        {
            if (this.unassociateItemCompartmentTypesDataSource == null)
            {
                return null;
            }

            return this.unassociateItemCompartmentTypesDataSource.Select(ict => ict.Error).FirstOrDefault(ict => !string.IsNullOrEmpty(ict));
        }

        public override async Task<bool> SaveAsync()
        {
            if (!this.IsValid())
            {
                return false;
            }

            this.IsBusy = true;

            var newItemCompartmentTypes = this.CreateBulk();
            var result = await this.compartmentTypeProvider.AddItemCompartmentTypesRangeAsync(newItemCompartmentTypes);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                                             Common.Resources.MasterData.ItemCompartmentTypesSavedSuccessfully,
                                             StatusType.Success));
                this.EventService.Invoke(new ModelChangedPubSubEvent(typeof(ItemCompartmentType).ToString(), null, HubEntityOperation.Created));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }

            this.IsBusy = false;

            return true;
        }

        protected override async Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.Title = string.Format(Ferretto.Common.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
            }

            await this.LoadUnassociateCompartmentTypesAsync();

            await base.OnAppearAsync();
        }

        private IEnumerable<ItemCompartmentType> CreateBulk()
        {
            var newItemComaratmentTypes = new List<ItemCompartmentType>();

            var filtered = this.UnassociateItemCompartmentTypesDataSource.Where(uct => uct.IsActive);

            foreach (var itemCompartmentType in filtered)
            {
                itemCompartmentType.ItemId = ((IModel<int>)this.Data).Id;
                newItemComaratmentTypes.Add(itemCompartmentType);
            }

            return newItemComaratmentTypes;
        }

        private bool IsValid()
        {
            if (this.unassociateItemCompartmentTypesDataSource == null)
            {
                return false;
            }

            if (this.unassociateItemCompartmentTypesDataSource.FirstOrDefault(ct => ct.MaxCapacity <= 0) is ItemCompartmentType itemCompartmentTypeError)
            {
                this.SelectedItemCompartmentType = itemCompartmentTypeError;
                return false;
            }

            return this.unassociateItemCompartmentTypesDataSource.FirstOrDefault(ict => ict.IsActive) != null;
        }

        private async Task LoadUnassociateCompartmentTypesAsync()
        {
            if (!(this.Data is ItemDetails itemDetails))
            {
                return;
            }

            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateCanSave));

            var itemsCompartmentTypesResult = await this.compartmentTypeProvider.GetAllUnassociatedByItemIdAsync(itemDetails.Id);
            if (itemsCompartmentTypesResult.Success &&
                itemsCompartmentTypesResult.Entity.Any())
            {
                this.UnassociateItemCompartmentTypesDataSource = new ObservableCollection<ItemCompartmentType>(itemsCompartmentTypesResult.Entity);
                this.HasUnassociatedItemCompartmentTypes = true;
            }
            else
            {
                this.HasUnassociatedItemCompartmentTypes = false;
                this.UnassociateItemCompartmentTypesDataSource = null;
            }
        }

        private void SelectedCompartmentType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || this.selectedItemCompartmentType == null)
            {
                return;
            }

            if (e.PropertyName == nameof(ItemCompartmentType.MaxCapacity))
            {
                this.selectedItemCompartmentType.IsActive = true;
            }

            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateError));
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateCanSave));
        }

        #endregion
    }
}
