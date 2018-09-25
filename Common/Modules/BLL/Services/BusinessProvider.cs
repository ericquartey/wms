using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class BusinessProvider
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();

        #endregion Fields

        #region Methods

        public IEnumerable<Item> GetAllClassAItems()
        {
            return Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllClassAItems());
        }

        public IEnumerable<Item> GetAllItems()
        {
            var allItems = Mapper.Map<IEnumerable<Item>>(this.dataService.GetAllItems()); // join con compartments
            // calcolo totalstock

            return allItems;
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

        public ItemDetails GetItemDetails(int itemId)
        {
            return this.dataService.GetItemDetails(itemId);
        }

        public int Save(ItemDetails item)
        {
            return 0;
        }

        internal IQueryable<Compartment> GetAllCompartments()
        {
            throw new NotImplementedException();
        }

        internal IQueryable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            throw new NotImplementedException();
        }

        internal IQueryable<Item> GetItemsWithAClass()
        {
            throw new NotImplementedException();
        }

        internal Int32 GetItemsWithAClassCount()
        {
            throw new NotImplementedException();
        }

        internal IQueryable<Item> GetItemsWithFIFO()
        {
            throw new NotImplementedException();
        }

        internal Int32 GetItemsWithFIFOCount()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
