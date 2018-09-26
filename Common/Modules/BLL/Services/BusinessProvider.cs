using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ferretto.Common.DataAccess;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class BusinessProvider : IBusinessProvider
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();

        #endregion Fields

        #region Methods

        public IEnumerable<Item> GetAllClassAItems()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllClassAItems());
        }

        public IEnumerable<Compartment> GetAllCompartments()
        {
            return null;
        }

        public int GetAllCompartmentsCount()
        {
            return 0;
        }

        public IEnumerable<Item> GetAllItems()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllItems());
        }

        public int GetAllItemsCount()
        {
            return this.dataService.GetAllItems().Count();
        }

        public IEnumerable<Item> GetAllItemsFIFO()
        {
            var allItems = Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllItems()); // join con compartments
            // calcolo totalstock

            return allItems;
        }

        public IEnumerable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            return null;
        }

        public ItemDetails GetItemDetails(int itemId)
        {
            return Mapper.Map<ItemDetails>(this.dataService.GetItemDetails(itemId));
        }

        public IEnumerable<Item> GetItemsWithAClass()
        {
            return null;
        }

        public int GetItemsWithAClassCount()
        {
            return 0;
        }

        public IEnumerable<Item> GetItemsWithFIFO()
        {
            return null;
        }

        public int GetItemsWithFIFOCount()
        {
            return 0;
        }

        public int Save(ItemDetails item)
        {
            return 0;
        }

        public int Save(CompartmentDetails compartment)
        {
            return 0;
        }

        #endregion Methods
    }
}
