using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class SchedulerRequestProvider : ISchedulerRequestProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.SchedulerRequest, bool>> OperationInsertFilter =
            request => (char)request.OperationType == (char)OperationType.Insertion;

        private static readonly Expression<Func<DataModels.SchedulerRequest, bool>> OperationWithdrawFilter =
            request => (char)request.OperationType == (char)OperationType.Withdrawal;

        private readonly IDatabaseContextService dataContextService;

        #endregion Fields

        #region Constructors

        public SchedulerRequestProvider(
            IDatabaseContextService dataContextService)
        {
            this.dataContextService = dataContextService;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> AddAsync(SchedulerRequest model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<SchedulerRequest> GetAll()
        {
            return this.dataContextService.Current.SchedulerRequests
                .Include(r => r.Bay)
                .Include(r => r.Area)
                .Include(r => r.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(r => r.List)
                .Include(r => r.ListRow)
                .Include(r => r.LoadingUnit)
                .Include(r => r.LoadingUnitType)
                .Include(m => m.MaterialStatus)
                .Include(m => m.PackageType)
                .Select(r => new SchedulerRequest
                {
                    BayDescription = r.Bay.Description,
                    ItemDescription = r.Item.Description,
                    ListDescription = r.List.Description,
                    ListRowDescription = r.ListRow.Code,
                    LoadingUnitDescription = r.LoadingUnit.Code,
                    IsInstant = r.IsInstant,
                    RequestedQuantity = r.RequestedQuantity,
                    OperationType = (OperationType)r.OperationType,

                    LoadingUnitTypeDescription = r.LoadingUnitType.Description,
                    RegistrationNumber = r.RegistrationNumber,
                    Lot = r.Lot,
                    DispatchedQuantity = r.DispatchedQuantity,
                    MaterialStatusDescription = r.MaterialStatus.Description,
                    Sub1 = r.Sub1,
                    Sub2 = r.Sub2,
                    CreationDate = r.CreationDate,
                    LastModificationDate = r.LastModificationDate,
                    PackageTypeDescription = r.PackageType.Description,
                    AreaDescription = r.Area.Name,

                    ItemUnitMeasure = r.Item.MeasureUnit.Description
                });
        }

        public int GetAllCount()
        {
            using (var dataContext = this.dataContextService.Current)
            {
                return dataContext.SchedulerRequests.Count();
            }
        }

        public Task<SchedulerRequest> GetByIdAsync(int id) => throw new NotSupportedException();

        public IQueryable<SchedulerRequest> GetWithOperationTypeInsertion()
        {
            return GetAllRequestsWithAggregations(this.dataContextService.Current, OperationInsertFilter);
        }

        public int GetWithOperationTypeInsertionCount()
        {
            return this.dataContextService.Current.SchedulerRequests.AsNoTracking().Count(OperationInsertFilter);
        }

        public IQueryable<SchedulerRequest> GetWithOperationTypeWithdrawal()
        {
            return GetAllRequestsWithAggregations(this.dataContextService.Current, OperationWithdrawFilter);
        }

        public int GetWithOperationTypeWithdrawalCount()
        {
            return this.dataContextService.Current.SchedulerRequests.AsNoTracking().Count(OperationWithdrawFilter);
        }

        public Task<OperationResult> SaveAsync(SchedulerRequest model)
        {
            throw new NotSupportedException();
        }

        private static IQueryable<SchedulerRequest> GetAllRequestsWithAggregations(DatabaseContext context, Expression<Func<DataModels.SchedulerRequest, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.SchedulerRequests
                .Where(actualWhereFunc)
                .Include(r => r.Bay)
                .Include(r => r.Item)
                .Include(r => r.List)
                .Include(r => r.ListRow)
                .Include(r => r.LoadingUnit)
                .Include(r => r.LoadingUnitType)
                .Select(r => new SchedulerRequest
                {
                    BayDescription = r.Bay.Description,
                    ItemDescription = r.Item.Description,
                    ListDescription = r.List.Description,
                    ListRowDescription = r.ListRow.Code,
                    LoadingUnitDescription = r.LoadingUnit.Code,
                    IsInstant = r.IsInstant,
                    RequestedQuantity = r.RequestedQuantity,
                    OperationType = (OperationType)r.OperationType,
                    LoadingUnitTypeDescription = r.LoadingUnitType.Description
                }).AsNoTracking();
        }

        #endregion Methods
    }
}
