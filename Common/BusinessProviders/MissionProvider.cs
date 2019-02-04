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
            mission => (char)mission.Status == (char)MissionStatus.Completed;

        private static readonly Expression<Func<DataModels.Mission, bool>> StatusNewFilter =
            mission => (char)mission.Status == (char)MissionStatus.New;

        private readonly IDatabaseContextService dataContextService;

        #endregion

        #region Constructors

        public MissionProvider(
            IDatabaseContextService dataContextService)
        {
            this.dataContextService = dataContextService;
        }

        #endregion

        #region Methods

        public static int Save(MissionDetails model) => throw new NotSupportedException();

        public Task<OperationResult> AddAsync(MissionDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<Mission> GetAll()
        {
            return this.dataContextService.Current.Missions
                 .Include(m => m.Bay)
                 .Include(m => m.Cell)
                 .ThenInclude(c => c.Aisle)
                .Include(m => m.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(m => m.ItemList)
                .Include(m => m.ItemListRow)
                .Include(m => m.LoadingUnit)
                .Include(m => m.MaterialStatus)
                .Include(m => m.PackageType)
                .Include(m => m.Compartment)
                .ThenInclude(c => c.CompartmentType)
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
                    CellDescription = m.Cell.Aisle.Name,
                    CompartmentType = string.Format(Common.Resources.MasterData.CompartmentTypeListFormatReduced, m.Compartment.CompartmentType.Width, m.Compartment.CompartmentType.Height),
                    MaterialStatusDescription = m.MaterialStatus.Description,
                    PackageTypeDescription = m.PackageType.Description,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,

                    ItemUnitMeasure = m.Item.MeasureUnit.Description
                });
        }

        public int GetAllCount()
        {
            using (var dataContext = this.dataContextService.Current)
            {
                return dataContext.Missions.Count();
            }
        }

        public Task<MissionDetails> GetByIdAsync(int id) => throw new NotSupportedException();

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

        public Task<OperationResult> SaveAsync(MissionDetails model) => throw new NotSupportedException();

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

        #endregion
    }
}
