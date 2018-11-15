using System;
using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Ferretto.Common.Modules.BLL.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public SchedulerRequestProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public int Add(SchedulerRequest model)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<SchedulerRequest> GetAll()
        {
            return this.dataContext.SchedulerRequests

                .Select(s =>
                    s.IsInstant ?
                    new InstantSchedulerRequest
                    {
                        Id = s.Id,
                        AreaId = s.AreaId,
                        BayId = s.BayId,
                        CreationDate = s.CreationDate,
                        ItemId = s.ItemId,
                        LoadingUnitId = s.LoadingUnitId,
                        LoadingUnitTypeId = s.LoadingUnitTypeId,
                        MaterialStatusId = s.MaterialStatusId,
                        Lot = s.Lot,
                        OperationType = s.OperationType.ToString(),
                        PackageTypeId = s.PackageTypeId,
                        RegistrationNumber = s.RegistrationNumber,
                        RequestedQuantity = s.RequestedQuantity,
                        Sub1 = s.Sub1,
                        Sub2 = s.Sub2
                    } as SchedulerRequest
                    :
                    new ListSchedulerRequest
                    {
                        Id = s.Id,
                        AreaId = s.AreaId,
                        BayId = s.BayId,
                        CreationDate = s.CreationDate,
                        OperationType = s.OperationType.ToString(),
                    } as SchedulerRequest);
        }

        public Area GetById(int id)
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            var areaDetails = context.Areas
                .Where(a => a.Id == id)
                .Select(a => new Area
                {
                    Id = a.Id,
                    Name = a.Name,
                })
                .Single();

            return areaDetails;
        }

        public int Save(SchedulerRequest model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Areas.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
