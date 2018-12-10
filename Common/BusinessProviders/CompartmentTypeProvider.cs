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

        #endregion Fields

        #region Constructors

        public CompartmentTypeProvider(
            IDatabaseContextService context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public async Task<Int32> Add(CompartmentType model)
        {
            //TODO: Task 823
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            var existing = dataContext.CompartmentTypes.SingleOrDefault(ct =>
            (ct.Width == model.Width || ct.Height == model.Height)
            &&
            (ct.Width == model.Height || ct.Height == model.Width));

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
                    return model.Id;
                }
            }

            return existing.Id;
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
