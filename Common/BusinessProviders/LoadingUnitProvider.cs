using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class LoadingUnitProvider : ILoadingUnitProvider
    {
        #region Fields

        private readonly ICellProvider cellProvider;
        private readonly ICompartmentProvider compartmentProvider;
        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public LoadingUnitProvider(
            ICellProvider cellProvider,
            ICompartmentProvider compartmentProvider,
            IDatabaseContextService dataContext,
            EnumerationProvider enumerationProvider)
        {
            this.cellProvider = cellProvider;
            this.compartmentProvider = compartmentProvider;
            this.dataContext = dataContext;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Task<int> Add(LoadingUnitDetails model)
        {
            throw new NotImplementedException();
        }

        public int Delete(int id)
        {
            throw new NotImplementedException();
        }

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
                    CellSide = l.Cell.Side.ToString(),
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                }).AsNoTracking();
        }

        public int GetAllCount()
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.LoadingUnits.Count();
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
                    CellPairing = (int)l.CellPairing,
                    CellPairingDetails = l.CellPairing.ToString(),
                    ReferenceType = l.Reference.ToString(),
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

        public LoadingUnitDetails GetById(int id)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var loadingUnitDetails = dataContext.LoadingUnits
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
                    CellPairing = (int)l.CellPairing,
                    CellPairingDetails = l.CellPairing.ToString(),
                    ReferenceType = l.Reference.ToString(),
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
                .Single();

                loadingUnitDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
                loadingUnitDetails.CellPositionChoices = this.enumerationProvider.GetAllCellPositions();
                loadingUnitDetails.LoadingUnitStatusChoices = this.enumerationProvider.GetAllLoadingUnitStatuses();
                loadingUnitDetails.LoadingUnitTypeChoices = this.enumerationProvider.GetAllLoadingUnitTypes();
                foreach (var compartment in this.compartmentProvider.GetByLoadingUnitId(id))
                {
                    loadingUnitDetails.AddCompartment(compartment);
                }

                loadingUnitDetails.CellPairingChoices =
                    ((DataModels.Pairing[])Enum.GetValues(typeof(DataModels.Pairing)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();
                loadingUnitDetails.ReferenceTypeChoices =
                    ((DataModels.ReferenceType[])Enum.GetValues(typeof(DataModels.ReferenceType)))
                    .Select(i => new EnumerationString(i.ToString(), i.ToString())).ToList();
                loadingUnitDetails.CellChoices = this.cellProvider.GetByAreaId(loadingUnitDetails.AreaId);

                return loadingUnitDetails;
            }
        }

        public bool HasAnyCompartments(int loadingUnitId)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Compartments.AsNoTracking().Any(l => l.LoadingUnitId == loadingUnitId);
            }
        }

        public int Save(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.LoadingUnits.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                foreach (var compartment in model.Compartments)
                {
                    var existingCompartment = dataContext.Compartments.Find(compartment.Id);
                    dataContext.Entry(existingCompartment).CurrentValues.SetValues(compartment);
                }

                return dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
