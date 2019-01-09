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
    public class MissionProvider : IMissionProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Mission, bool>> StatusCompletedFilter =
            mission => (char)mission.Status == (char)(MissionStatus.Completed);

        private static readonly Expression<Func<DataModels.Mission, bool>> StatusNewFilter =
            mission => (char)mission.Status == (char)(MissionStatus.New);

        private readonly IDatabaseContextService dataContextService;

        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public MissionProvider(
            IDatabaseContextService dataContextService,
            EnumerationProvider enumerationProvider)
        {
            this.dataContextService = dataContextService;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<OperationResult> Add(MissionDetails model)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> AddAsync(MissionDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Mission> GetAll()
        {
            return this.dataContextService.Current.Missions
                 .Include(m => m.Bay)
                .Include(m => m.Item)
                .Include(m => m.ItemList)
                .Include(m => m.ItemListRow)
                .Include(m => m.LoadingUnit)
                .Select(m => new Mission
                {
                    BayDescription = m.Bay.Description,
                    ItemDescription = m.Item.Description,
                    ItemListDescription = m.ItemList.Description,
                    ItemListRowDescription = m.ItemListRow.Code,
                    LoadingUnitDescription = m.LoadingUnit.Code,
                    Priority = m.Priority,
                    RequiredQuantity = m.RequiredQuantity,
                    Status = (MissionStatus)m.Status,
                    Type = (MissionType)m.Type,

                    CreationDate = m.CreationDate,
                    LastModificationDate = m.LastModificationDate,
                    RegistrationNumber = m.RegistrationNumber,
                    Lot = m.Lot,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeId = m.PackageTypeId,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2
                });
        }

        public int GetAllCount()
        {
            using (var dataContext = this.dataContextService.Current)
            {
                return dataContext.Missions.Count();
            }
        }

        public Task<MissionDetails> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Mission> GetWithStatusCompleted()
        {
            return GetAllMissionsWithAggregations(this.dataContextService.Current, StatusCompletedFilter);
        }

        public int GetWithStatusCompletedCount()
        {
            return this.dataContextService.Current.Missions.AsNoTracking().Count(StatusCompletedFilter);
        }

        public IQueryable<Mission> GetWithStatusNew()
        {
            return GetAllMissionsWithAggregations(this.dataContextService.Current, StatusNewFilter);
        }

        public int GetWithStatusNewCount()
        {
            return this.dataContextService.Current.Missions.AsNoTracking().Count(StatusNewFilter);
        }

        public int Save(MissionDetails model)
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> SaveAsync(MissionDetails model)
        {
            throw new NotImplementedException();
        }

        private static IQueryable<Mission> GetAllMissionsWithAggregations(DatabaseContext context, Expression<Func<DataModels.Mission, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Missions
             .Where(actualWhereFunc)
             .Include(m => m.Bay)
             .Include(m => m.Item)
             .Include(m => m.ItemList)
             .Include(m => m.ItemListRow)
             .Include(m => m.LoadingUnit)
             .Select(m => new Mission
             {
                 BayDescription = m.Bay.Description,
                 ItemDescription = m.Item.Description,
                 ItemListDescription = m.ItemList.Description,
                 ItemListRowDescription = m.ItemListRow.Code,
                 LoadingUnitDescription = m.LoadingUnit.Code,
                 Priority = m.Priority,
                 RequiredQuantity = m.RequiredQuantity,
                 Status = (MissionStatus)m.Status,
                 Type = (MissionType)m.Type
             }).AsNoTracking();
        }

        #endregion Methods
    }
}
