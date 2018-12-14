using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<OperationResult> Add(CompartmentType model, int? itemId, int? maxCapacity)
        {
            //TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            var existing = dataContext.CompartmentTypes.SingleOrDefault(ct =>
            (ct.Width == model.Width && ct.Height == model.Height)
            ||
            (ct.Width == model.Height && ct.Height == model.Width));

            var compartmentTypeId = -1;
            if (existing == null)
            {
                var entry = dataContext.CompartmentTypes.Add(new DataModels.CompartmentType
                {
                    Description = model.Description,
                    Height = model.Height,
                    Width = model.Width
                });

                var changedEntitiesCount = await dataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    model.Id = entry.Entity.Id;
                    compartmentTypeId = model.Id;
                }
                else
                {
                    return new OperationResult(false, description: string.Format(Resources.Errors.NotAddDB, nameof(CompartmentType)));
                }
            }
            else
            {
                compartmentTypeId = existing.Id;
            }

            //Add Association ItemCompartmentType
            if (itemId.HasValue)
            {
                var itemCompartmentTypeId = await this.itemCompartmentTypeProvider.Add(new ItemCompartmentType
                {
                    ItemId = itemId.Value,
                    MaxCapacity = maxCapacity,
                    CompartmentTypeId = compartmentTypeId
                });

                var addItemCompartmentTypeCount = await dataContext.SaveChangesAsync();
                if (addItemCompartmentTypeCount < 1)
                {
                    //TODO
                    return new OperationResult(false, description: string.Format(Resources.Errors.NotAddDB, nameof(ItemCompartmentType)));
                }
            }

            return new OperationResult(true, entityId: compartmentTypeId);
        }

        public Task<OperationResult> Add(CompartmentType model)
        {
            return this.Add(model, null, null);
        }

        public Int32 Delete(Int32 id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<CompartmentType> GetAll()
        {
            throw new NotImplementedException();
        }

        public Int32 GetAllCount()
        {
            throw new NotImplementedException();
        }

        public CompartmentType GetById(Int32 id)
        {
            throw new NotImplementedException();
        }

        public Int32 Save(CompartmentType model)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
