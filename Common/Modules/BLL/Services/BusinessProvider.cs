using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
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
            return Mapper.Map<ItemDetails>(this.dataService.GetItemDetails(itemId));
        }

        public int Save(ItemDetails item)
        {
            return 0;
        }

        public int Save(CompartmentDetails compartment)
        {
            return 0;
        }

        public IEnumerable<Compartment> GetAllCompartments()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Compartment> GetCompartmentsByItemId(int itemId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Item> GetItemsWithAClass()
        {
            throw new NotImplementedException();
        }

        public int GetItemsWithAClassCount()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Item> GetItemsWithFIFO()
        {
            throw new NotImplementedException();
        }

        public int GetItemsWithFIFOCount()
        {
            throw new NotImplementedException();
        }

        public int GetAllCompartmentsCount()
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
