using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public class ItemCompartmentTypeProvider : IItemCompartmentTypeProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContextService;

        #endregion

        #region Constructors

        public ItemCompartmentTypeProvider(
            IDatabaseContextService dataContextService)
        {
            this.dataContextService = dataContextService;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult> AddAsync(ItemCompartmentType model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dataContext = this.dataContextService.Current)
                {
                    var itemCompartmentType = dataContext.ItemsCompartmentTypes
                    .SingleOrDefault(ict =>
                        ict.CompartmentTypeId == model.CompartmentTypeId
                        &&
                        ict.ItemId == model.ItemId);

                    if (itemCompartmentType == null)
                    {
                        dataContext.ItemsCompartmentTypes.Add(new DataModels.ItemCompartmentType
                        {
                            CompartmentTypeId = model.CompartmentTypeId,
                            ItemId = model.ItemId,
                            MaxCapacity = model.MaxCapacity
                        });

                        var changedEntitiesCount = await dataContext.SaveChangesAsync();
                        if (changedEntitiesCount <= 0)
                        {
                            return new OperationResult(false);
                        }
                    }
                    else
                    {
                        dataContext.Entry(itemCompartmentType).CurrentValues.SetValues(model);
                        await dataContext.SaveChangesAsync();
                    }

                    return new OperationResult(true);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<ItemCompartmentType> GetAll() => throw new NotSupportedException();

        public int GetAllCount() => throw new NotSupportedException();

        public Task<ItemCompartmentType> GetByIdAsync(int id) => throw new NotSupportedException();

        public ItemCompartmentType GetNew()
        {
            throw new NotSupportedException();
        }

        public Task<IOperationResult> SaveAsync(ItemCompartmentType model) => throw new NotSupportedException();

        #endregion
    }
}
