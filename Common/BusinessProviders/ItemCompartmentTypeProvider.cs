using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
    {
        #region Methods

        public Task<Int32> Add(ItemCompartmentType model)
        {
            throw new NotImplementedException();
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemCompartmentType> GetAll()
        {
            throw new NotImplementedException();
        }

        public Int32 GetAllCount()
        {
            throw new NotImplementedException();
        }

        public ItemCompartmentType GetById(Int32 id)
        {
            throw new NotImplementedException();
        }

        public Int32 Save(ItemCompartmentType model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
