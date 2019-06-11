using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.App.Modules.MasterData
{
    public class CompartmentTypesToItemViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private ItemCompartmentType selectedCompartmentType;

        private string title;

        private IEnumerable<ItemCompartmentType> unassociateCompartmentTypesDataSource;

        #endregion

        #region Constructors

        public CompartmentTypesToItemViewModel(ICompartmentTypeProvider compartmentTypeProvider)
        {
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion

        #region Properties

        public ItemCompartmentType SelectedCompartmentType
        {
            get => this.selectedCompartmentType;
            set => this.SetProperty(ref this.selectedCompartmentType, value);
        }

        public string Title => this.title;

        public IEnumerable<ItemCompartmentType> UnassociateCompartmentTypesDataSource
        {
            get => this.unassociateCompartmentTypesDataSource;
            set => this.SetProperty(ref this.unassociateCompartmentTypesDataSource, value);
        }

        #endregion

        #region Methods

        protected override async Task OnAppearAsync()
        {
            await this.LoadItemAreasAsync();
        }

        private async Task LoadItemAreasAsync()
        {
            if (!(this.Data is ItemDetails itemDetails))
            {
                return;
            }

            this.title = string.Format("Associate Compartment Type to This Item {0}", itemDetails.Code);
            this.RaisePropertyChanged(nameof(this.Title));

            var allowedItemAreasResult = await this.compartmentTypeProvider.GetAllUnassociatedByItemIdAsync(itemDetails.Id);
            if (allowedItemAreasResult.Success)
            {
                this.UnassociateCompartmentTypesDataSource = allowedItemAreasResult.Entity;
            }
            else
            {
                this.UnassociateCompartmentTypesDataSource = null;
            }
        }

        #endregion
    }
}
