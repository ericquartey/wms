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

        public IEnumerable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            return Mapper.Map<IEnumerable<Compartment>>(this.dataService.GetCompartmentsByItemId(itemId));
        }

        public ItemDetails GetItemDetails(int itemId)
        {
            return Mapper.Map<ItemDetails>(this.dataService.GetItemDetails(itemId));
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
