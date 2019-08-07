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
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.App.Modules.MasterData
{
    public class ChooseCompartmentTypesStepViewModel : WmsWizardStepViewModel
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private bool hasUnassociatedItemCompartmentTypes;

        private bool isBusy;

        private ItemCompartmentType selectedItemCompartmentType;

        private ObservableCollection<ItemCompartmentType> unassociatedItemCompartmentTypesDataSource;

        #endregion

        #region Constructors

        public ChooseCompartmentTypesStepViewModel(ICompartmentTypeProvider compartmentTypeProvider)
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
                var lastSelection = this.selectedItemCompartmentType;

                if (this.SetProperty(ref this.selectedItemCompartmentType, value) &&
                    this.selectedItemCompartmentType != null)
                {
                    if (lastSelection != null)
                    {
                        lastSelection.PropertyChanged -= this.SelectedCompartmentType_PropertyChanged;
                    }

                    this.selectedItemCompartmentType.PropertyChanged += this.SelectedCompartmentType_PropertyChanged;
                }
            }
        }

        public ObservableCollection<ItemCompartmentType> UnassociatedItemCompartmentTypesDataSource
        {
            get => this.unassociatedItemCompartmentTypesDataSource;
            set => this.SetProperty(ref this.unassociatedItemCompartmentTypesDataSource, value);
        }

        #endregion

        #region Methods

        public override bool CanGoToNextView()
        {
            return false;
        }

        public override bool CanSave()
        {
            return true;
        }

        public override string GetError()
        {
            if (this.unassociatedItemCompartmentTypesDataSource == null)
            {
                return null;
            }

            return this.unassociatedItemCompartmentTypesDataSource.Select(ict => ict.Error).FirstOrDefault(ict => !string.IsNullOrEmpty(ict));
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
                                             Resources.MasterData.ItemCompartmentTypesSavedSuccessfully,
                                             StatusType.Success));
            }
            else
            {
                this.SetValidation(true);
                this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateError));
            }

            this.IsBusy = false;

            return result.Success;
        }

        protected override async Task OnAppearAsync()
        {
            if (this.Data is ItemDetails itemDetails)
            {
                this.Title = string.Format(App.Resources.Title.AssociateCompartmentTypeToThisItem, itemDetails.Code);
            }

            await this.LoadUnassociateCompartmentTypesAsync();

            await base.OnAppearAsync();
        }

        private IEnumerable<ItemCompartmentType> CreateBulk()
        {
            var newItemComparatmentTypes = new List<ItemCompartmentType>();

            var filtered = this.UnassociatedItemCompartmentTypesDataSource.Where(uct => uct.IsActive);

            foreach (var itemCompartmentType in filtered)
            {
                itemCompartmentType.ItemId = ((IModel<int>)this.Data).Id;
                newItemComparatmentTypes.Add(itemCompartmentType);
            }

            return newItemComparatmentTypes;
        }

        private bool IsValid()
        {
            if (this.unassociatedItemCompartmentTypesDataSource == null)
            {
                return false;
            }

            if (this.unassociatedItemCompartmentTypesDataSource.Any(ct => !string.IsNullOrEmpty(ct.Error)))
            {
                return false;
            }

            return this.unassociatedItemCompartmentTypesDataSource.FirstOrDefault(ict => ict.IsActive) != null;
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
                this.UnassociatedItemCompartmentTypesDataSource = new ObservableCollection<ItemCompartmentType>(itemsCompartmentTypesResult.Entity);
                this.HasUnassociatedItemCompartmentTypes = true;
            }
            else
            {
                this.HasUnassociatedItemCompartmentTypes = false;
                this.UnassociatedItemCompartmentTypesDataSource = null;
            }
        }

        private void NotifyToUpdate()
        {
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateError));
            this.EventService.Invoke(new StepsPubSubEvent(CommandExecuteType.UpdateCanSave));
        }

        private void SelectedCompartmentType_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null || this.selectedItemCompartmentType == null)
            {
                return;
            }

            if (e.PropertyName == nameof(ItemCompartmentType.MaxCapacity))
            {
                if (this.selectedItemCompartmentType.MaxCapacity.HasValue)
                {
                    this.selectedItemCompartmentType.IsActive = true;
                }

                this.NotifyToUpdate();
            }

            if (e.PropertyName == nameof(ItemCompartmentType.IsActive))
            {
                if (this.selectedItemCompartmentType.IsActive)
                {
                    this.selectedItemCompartmentType.MaxCapacity = 0;
                    this.NotifyToUpdate();
                }
                else
                {
                    this.selectedItemCompartmentType.MaxCapacity = null;
                }
            }
        }

        private void SetValidation(bool enable)
        {
            foreach (var unassociatedItemCompartmentType in this.unassociatedItemCompartmentTypesDataSource)
            {
                unassociatedItemCompartmentType.IsValidationEnabled = enable;
            }
        }

        #endregion
    }
}
