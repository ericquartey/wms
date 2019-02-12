using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class LoadingUnitProvider : ILoadingUnitProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.LoadingUnit, bool>> AreaManualFilter =
            lu => lu.CellPositionId == 1; // AREA MANUAL

        private static readonly Expression<Func<DataModels.LoadingUnit, bool>> AreaVertimagFilter =
            lu => lu.CellPositionId == 2; // AREA VERTIMAG

        private static readonly Expression<Func<DataModels.LoadingUnit, bool>> StatusAvailableFilter =
            lu => lu.LoadingUnitStatusId == "A"; // STATUS Available

        private static readonly Expression<Func<DataModels.LoadingUnit, bool>> StatusBlockedFilter =
            lu => lu.LoadingUnitStatusId == "B"; // STATUS Blocked

        private static readonly Expression<Func<DataModels.LoadingUnit, bool>> StatusUsedFilter =
            lu => lu.LoadingUnitStatusId == "U"; // STATUS Used

        private readonly IAbcClassesProvider abcClassesProvider;

        private readonly ICellProvider cellProvider;

        private readonly ICompartmentProvider compartmentProvider;

        private readonly IDatabaseContextService dataContext;

        private readonly EnumerationProvider enumerationProvider;

        #endregion

        #region Constructors

        public LoadingUnitProvider(
            ICellProvider cellProvider,
            ICompartmentProvider compartmentProvider,
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider,
            IAbcClassesProvider abcClassesProvider)
        {
            this.cellProvider = cellProvider;
            this.compartmentProvider = compartmentProvider;
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
            this.abcClassesProvider = abcClassesProvider;
        }

        #endregion

        #region Methods

        public Task<OperationResult> AddAsync(LoadingUnitDetails model) => throw new NotSupportedException();

        public Task<int> DeleteAsync(int id) => throw new NotSupportedException();

        public IQueryable<LoadingUnit> GetAll()
        {
            return this.dataContext.Current.LoadingUnits
                .Include(l => l.LoadingUnitType)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Select(l => new LoadingUnit
                {
                    Id = l.Id,
                    Code = l.Code,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    AbcClassDescription = l.AbcClass.Description,
                    AreaName = l.Cell.Aisle.Area.Name,
                    AisleName = l.Cell.Aisle.Name,
                    CellFloor = l.Cell.Floor,
                    CellColumn = l.Cell.Column,
                    CellSide = (Side)l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                }).AsNoTracking();
        }

        public int GetAllCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.Count();
            }
        }

        public IQueryable<LoadingUnitDetails> GetByCellId(int id)
        {
            return this.dataContext.Current.LoadingUnits
                .Where(l => l.CellId == id)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.LoadingUnitType)
                .ThenInclude(l => l.LoadingUnitSizeClass)
                .Include(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .Select(l => new LoadingUnitDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    AbcClassId = l.AbcClassId,
                    AbcClassDescription = l.AbcClass.Description,
                    CellPositionId = l.CellPositionId,
                    CellPositionDescription = l.CellPosition.Description,
                    LoadingUnitStatusId = l.LoadingUnitStatusId,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    LoadingUnitTypeId = l.LoadingUnitTypeId,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    Width = l.LoadingUnitType.LoadingUnitSizeClass.Width,
                    Length = l.LoadingUnitType.LoadingUnitSizeClass.Length,
                    Note = l.Note,
                    IsCellPairingFixed = l.IsCellPairingFixed,
                    ReferenceType = (ReferenceType)l.Reference,
                    Height = l.Height,
                    Weight = l.Weight,
                    HandlingParametersCorrection = l.HandlingParametersCorrection,
                    LoadingUnitTypeHasCompartments = l.LoadingUnitType.HasCompartments,
                    CreationDate = l.CreationDate,
                    LastHandlingDate = l.LastHandlingDate,
                    InventoryDate = l.InventoryDate,
                    LastPickDate = l.LastPickDate,
                    LastStoreDate = l.LastStoreDate,
                    InCycleCount = l.InCycleCount,
                    OutCycleCount = l.OutCycleCount,
                    OtherCycleCount = l.OtherCycleCount,
                    CellId = l.CellId,
                    AisleId = l.Cell.AisleId,
                    AreaId = l.Cell.Aisle.AreaId,
                })
                .AsNoTracking();
        }

        public async Task<LoadingUnitDetails> GetByIdAsync(int id)
        {
            var dc = this.dataContext.Current;

            var loadingUnitDetails = await dc.LoadingUnits
                .Where(l => l.Id == id)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.LoadingUnitType)
                .ThenInclude(l => l.LoadingUnitSizeClass)
                .Include(l => l.Cell)
                .ThenInclude(c => c.Aisle)
                .Select(l => new LoadingUnitDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    AbcClassId = l.AbcClassId,
                    AbcClassDescription = l.AbcClass.Description,
                    CellPositionId = l.CellPositionId,
                    CellPositionDescription = l.CellPosition.Description,
                    LoadingUnitStatusId = l.LoadingUnitStatusId,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    LoadingUnitTypeId = l.LoadingUnitTypeId,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    Width = l.LoadingUnitType.LoadingUnitSizeClass.Width,
                    Length = l.LoadingUnitType.LoadingUnitSizeClass.Length,
                    Note = l.Note,
                    IsCellPairingFixed = l.IsCellPairingFixed,
                    ReferenceType = (ReferenceType)l.Reference,
                    Height = l.Height,
                    Weight = l.Weight,
                    HandlingParametersCorrection = l.HandlingParametersCorrection,
                    LoadingUnitTypeHasCompartments = l.LoadingUnitType.HasCompartments,
                    CreationDate = l.CreationDate,
                    LastHandlingDate = l.LastHandlingDate,
                    InventoryDate = l.InventoryDate,
                    LastPickDate = l.LastPickDate,
                    LastStoreDate = l.LastStoreDate,
                    InCycleCount = l.InCycleCount,
                    OutCycleCount = l.OutCycleCount,
                    OtherCycleCount = l.OtherCycleCount,
                    CellId = l.CellId,
                    AisleId = l.Cell.AisleId,
                    AreaId = l.Cell.Aisle.AreaId,
                })
                .SingleAsync();

            loadingUnitDetails.AbcClassChoices = await this.abcClassesProvider.GetAllAsync();
            loadingUnitDetails.CellPositionChoices = this.enumerationProvider.GetAllCellPositions();
            loadingUnitDetails.LoadingUnitStatusChoices = this.enumerationProvider.GetAllLoadingUnitStatuses();
            loadingUnitDetails.LoadingUnitTypeChoices = this.enumerationProvider.GetAllLoadingUnitTypes();
            foreach (var compartment in this.compartmentProvider.GetByLoadingUnitId(id))
            {
                loadingUnitDetails.AddCompartment(compartment);
            }

            loadingUnitDetails.CellChoices = this.cellProvider.GetByAreaId(loadingUnitDetails.AreaId);

            return loadingUnitDetails;
        }

        public LoadingUnitDetails GetNew()
        {
            throw new NotImplementedException();
        }

        public IQueryable<LoadingUnit> GetWithAreaManual()
        {
            return GetAllLoadingUnitsWithAggregations(this.dataContext.Current, AreaManualFilter);
        }

        public int GetWithAreaManualCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Count(AreaManualFilter);
            }
        }

        public IQueryable<LoadingUnit> GetWithAreaVertimag()
        {
            return GetAllLoadingUnitsWithAggregations(this.dataContext.Current, AreaVertimagFilter);
        }

        public int GetWithAreaVertimagCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Count(AreaVertimagFilter);
            }
        }

        public IQueryable<LoadingUnit> GetWithStatusAvailable()
        {
            return GetAllLoadingUnitsWithAggregations(this.dataContext.Current, StatusAvailableFilter);
        }

        public int GetWithStatusAvailableCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Count(StatusAvailableFilter);
            }
        }

        public IQueryable<LoadingUnit> GetWithStatusBlocked()
        {
            return GetAllLoadingUnitsWithAggregations(this.dataContext.Current, StatusBlockedFilter);
        }

        public int GetWithStatusBlockedCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Count(StatusBlockedFilter);
            }
        }

        public IQueryable<LoadingUnit> GetWithStatusUsed()
        {
            return GetAllLoadingUnitsWithAggregations(this.dataContext.Current, StatusUsedFilter);
        }

        public int GetWithStatusUsedCount()
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.LoadingUnits.AsNoTracking().Count(StatusUsedFilter);
            }
        }

        public bool HasAnyCompartments(int loadingUnitId)
        {
            using (var dc = this.dataContext.Current)
            {
                return dc.Compartments.AsNoTracking().Any(l => l.LoadingUnitId == loadingUnitId);
            }
        }

        public async Task<OperationResult> SaveAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                using (var dc = this.dataContext.Current)
                {
                    var existingModel = dc.LoadingUnits.Find(model.Id);

                    dc.Entry(existingModel).CurrentValues.SetValues(model);

                    foreach (var compartment in model.Compartments)
                    {
                        var existingCompartment = dc.Compartments.Find(compartment.Id);
                        dc.Entry(existingCompartment).CurrentValues.SetValues(compartment);
                    }

                    var changedEntityCount = await dc.SaveChangesAsync();

                    return new OperationResult(changedEntityCount > 0);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        private static IQueryable<LoadingUnit> GetAllLoadingUnitsWithAggregations(DatabaseContext context, Expression<Func<DataModels.LoadingUnit, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.LoadingUnits
                .Include(l => l.LoadingUnitType)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Where(actualWhereFunc)
                .Select(l => new LoadingUnit
                {
                    Id = l.Id,
                    Code = l.Code,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    AbcClassDescription = l.AbcClass.Description,
                    AreaName = l.Cell.Aisle.Area.Name,
                    AisleName = l.Cell.Aisle.Name,
                    CellFloor = l.Cell.Floor,
                    CellColumn = l.Cell.Column,
                    CellSide = (Side)l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                }).AsNoTracking();
        }

        #endregion
    }
}
