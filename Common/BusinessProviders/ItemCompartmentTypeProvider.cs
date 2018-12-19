using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        #endregion Fields

        #region Constructors

        public ItemCompartmentTypeProvider(
            IDatabaseContextService context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public async Task<OperationResult> Add(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            try
            {
                var dataContext = this.dataContext.Current;
                var existing = dataContext.ItemsCompartmentTypes.SingleOrDefault(
                    ict =>
                    (ict.CompartmentTypeId == model.CompartmentTypeId
                    && ict.ItemId == model.ItemId));

                if (existing == null)
                {
                    var entry = dataContext.ItemsCompartmentTypes.Add(new DataModels.ItemCompartmentType
                    {
                        CompartmentTypeId = model.CompartmentTypeId,
                        ItemId = model.ItemId,
                        MaxCapacity = model.MaxCapacity
                    });

                    var changedEntitiesCount = await dataContext.SaveChangesAsync();
                    if (changedEntitiesCount <= 0)
                    {
                        return new OperationResult(false, description: string.Format(Resources.Errors.NotAddDB, nameof(ItemCompartmentType)));
                    }
                }
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ItemCompartmentType> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            throw new NotImplementedException();
        }

        public Task<ItemCompartmentType> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int Save(ItemCompartmentType model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
