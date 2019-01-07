using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;

namespace Ferretto.Common.BusinessProviders
{
    public class CompartmentTypeProvider : ICompartmentTypeProvider
    {
        #region Fields

        private readonly IDatabaseContextService dataContext;

        private readonly ItemCompartmentTypeProvider itemCompartmentTypeProvider;

        #endregion Fields

        #region Constructors

        public CompartmentTypeProvider(
            IDatabaseContextService context,
            ItemCompartmentTypeProvider itemCompartmentTypeProvider)
        {
            this.dataContext = context;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
        }

        #endregion Constructors

        #region Methods

        public async Task<OperationResult> AddAsync(CompartmentType model, int? itemId, int? maxCapacity)
        {
            //TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var dataContext = this.dataContext.Current;
                var compartmentType = dataContext.CompartmentTypes
                    .SingleOrDefault(ct =>
                        (ct.Width == model.Width && ct.Height == model.Height)
                        ||
                        (ct.Width == model.Height && ct.Height == model.Width));

                if (compartmentType == null)
                {
                    var entry = dataContext.CompartmentTypes.Add(new DataModels.CompartmentType
                    {
                        Height = model.Height,
                        Width = model.Width
                    });

                    var changedEntitiesCount = await dataContext.SaveChangesAsync();
                    if (changedEntitiesCount > 0)
                    {
                        compartmentType = entry.Entity;
                        model.Id = entry.Entity.Id;
                    }
                    else
                    {
                        return new OperationResult(false);
                    }
                }

                if (itemId.HasValue)
                {
                    var result = await this.itemCompartmentTypeProvider.AddAsync(
                        new ItemCompartmentType
                        {
                            ItemId = itemId.Value,
                            MaxCapacity = maxCapacity,
                            CompartmentTypeId = compartmentType.Id
                        });

                    if (result.Success)
                    {
                        await dataContext.SaveChangesAsync();
                    }
                    else
                    {
                        return new OperationResult(false);
                    }
                }

                return new OperationResult(true, entityId: compartmentType.Id);
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        public Task<OperationResult> AddAsync(CompartmentType model)
        {
            return this.AddAsync(model, null, null);
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CompartmentType> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            throw new NotImplementedException();
        }

        public Task<CompartmentType> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> SaveAsync(CompartmentType model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
