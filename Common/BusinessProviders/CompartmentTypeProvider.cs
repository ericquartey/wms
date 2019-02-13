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

        #endregion

        #region Constructors

        public CompartmentTypeProvider(
            IDatabaseContextService context,
            ItemCompartmentTypeProvider itemCompartmentTypeProvider)
        {
            this.dataContext = context;
            this.itemCompartmentTypeProvider = itemCompartmentTypeProvider;
        }

        #endregion

        #region Methods

        public async Task<OperationResult> AddAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null)
        {
            // TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var compartmentType = dc.CompartmentTypes
                        .SingleOrDefault(ct =>
                            (ct.Width == model.Width && ct.Height == model.Height)
                            ||
                            (ct.Width == model.Height && ct.Height == model.Width));

                    if (compartmentType == null)
                    {
                        var entry = dc.CompartmentTypes.Add(new DataModels.CompartmentType
                        {
                            Height = model.Height,
                            Width = model.Width
                        });

                        var changedEntitiesCount = await dc.SaveChangesAsync();
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
                            await dc.SaveChangesAsync();
                        }
                        else
                        {
                            return new OperationResult(false);
                        }
                    }

                    return new OperationResult(true, entityId: compartmentType.Id);
                }
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

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<CompartmentType> GetAll() => throw new NotSupportedException();

        public int GetAllCount() => throw new NotSupportedException();

        public Task<CompartmentType> GetByIdAsync(int id) => throw new NotSupportedException();

        public CompartmentType GetNew()
        {
            throw new NotSupportedException();
        }

        public Task<OperationResult> SaveAsync(CompartmentType model) => throw new NotSupportedException();

        #endregion
    }
}
