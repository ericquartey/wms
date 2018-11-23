using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public MissionProvider(DatabaseContext context)
        {
            this.dataContext = context;
        }

        #endregion Constructors

        #region Methods

        public Task<int> Add(Mission model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Mission> GetAll()
        {
            throw new NotImplementedException();
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Cells.AsNoTracking().Count();
            }
        }

        public Mission GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int Save(Mission model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Cells.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
