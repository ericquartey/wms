using System.Collections.Generic;
using AutoMapper;
using Ferretto.Common.DataAccess;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class MasterDataProvider : IBusinessProvider
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();

        #endregion Fields

        #region Methods

        public IEnumerable<Compartment> GetAllCompartments()
        {
            return Mapper.Map<IEnumerable<Compartment>>(this.dataService.GetAllCompartments());
        }

        public int GetAllCompartmentsCount()
        {
            return this.dataService.GetAllCompartmentsCount();
        }

        public IEnumerable<Item> GetAllItems()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllItems());
        }

        public int GetAllItemsCount()
        {
            return this.dataService.GetAllItemsCount();
        }

        public CompartmentDetails GetCompartmentDetails(int compartmentId)
        {
            var compartmentDetails = Mapper.Map<CompartmentDetails>(this.dataService.GetCompartmentDetails(compartmentId));

            compartmentDetails.CompartmentStatusChoices = Mapper.Map<IEnumerable<CompartmentStatus>>(this.dataService.GetAllCompartmentStatuses());
            compartmentDetails.CompartmentTypeChoices = Mapper.Map<IEnumerable<CompartmentType>>(this.dataService.GetAllCompartmentTypes());
            compartmentDetails.MaterialStatusChoices = Mapper.Map<IEnumerable<MaterialStatus>>(this.dataService.GetAllMaterialStatuses());
            compartmentDetails.PackageTypeChoices = Mapper.Map<IEnumerable<PackageType>>(this.dataService.GetAllPackageTypes());

            return compartmentDetails;
        }

        public IEnumerable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            return Mapper.Map<IEnumerable<Compartment>>(this.dataService.GetCompartmentsByItemId(itemId));
        }

        public ItemDetails GetItemDetails(int itemId)
        {
            var itemDetails = Mapper.Map<ItemDetails>(this.dataService.GetItemDetails(itemId));

            itemDetails.AbcClassChoices = Mapper.Map<IEnumerable<AbcClass>>(this.dataService.GetAllAbcClasses());
            itemDetails.MeasureUnitChoices = Mapper.Map<IEnumerable<MeasureUnit>>(this.dataService.GetAllMeasureUnits());
            itemDetails.ItemManagementTypeChoices = Mapper.Map<IEnumerable<ItemManagementType>>(this.dataService.GetAllItemManagementTypes());

            return itemDetails;
        }

        public IEnumerable<Item> GetItemsWithAClass()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetItemsWithAClass());
        }

        public int GetItemsWithAClassCount()
        {
            return this.dataService.GetItemsWithAClassCount();
        }

        public IEnumerable<Item> GetItemsWithFifo()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetItemsWithFifo());
        }

        public int GetItemsWithFifoCount()
        {
            return this.dataService.GetItemsWithFifoCount();
        }

        public int Save(ItemDetails item)
        {
            return this.dataService.SaveItem(item);
        }

        public int Save(CompartmentDetails compartment)
        {
            return this.dataService.SaveCompartment(compartment);
        }

        #endregion Methods
    }
}
