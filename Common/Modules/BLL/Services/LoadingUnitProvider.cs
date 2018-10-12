using System;
using System.Linq;
using Ferretto.Common.EF;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class LoadingUnitProvider : ILoadingUnitProvider
    {
        #region Fields

        private readonly CompartmentProvider compartmentProvider;
        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public LoadingUnitProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;

            //TODO: use interface for CompartmentProvider and EnumerationProvider
            this.enumerationProvider = new EnumerationProvider(dataContext);
            this.compartmentProvider = new CompartmentProvider(dataContext);
        }

        #endregion Constructors

        #region Methods

        public IQueryable<LoadingUnit> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return context.LoadingUnits
                .Include(l => l.LoadingUnitType)
                .Include(l => l.LoadingUnitStatus)
                .Include(l => l.AbcClass)
                .Include(l => l.CellPosition)
                .Select(l => new LoadingUnit(l.Id)
                {
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
            return this.dataContext.LoadingUnits.Count();
        }

        public LoadingUnitDetails GetById(int id)
        {
            var loadingUnitDetails = this.dataContext.LoadingUnits
                .Where(l => l.Id == id)
                .Include(l => l.LoadingUnitType)
                .ThenInclude(l => l.LoadingUnitSizeClass)
                .Select(l => ProjectLoadingUnitDetails(l))
                .Single();

            loadingUnitDetails.AbcClassChoices = this.enumerationProvider.GetAllAbcClasses();
            loadingUnitDetails.CellPositionChoices = this.enumerationProvider.GetAllCellPositions();
            loadingUnitDetails.LoadingUnitStatusChoices = this.enumerationProvider.GetAllLoadingUnitStatuses();
            loadingUnitDetails.LoadingUnitTypeChoices = this.enumerationProvider.GetAllLoadingUnitTypes();
            foreach(var compartment in this.compartmentProvider.GetByLoadingUnitId(id)){
                loadingUnitDetails.AddCompartment(compartment);
            }

            return loadingUnitDetails;
        }

        public int Save(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.LoadingUnits.Find(model.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            return this.dataContext.SaveChanges();
        }

        private static LoadingUnitDetails ProjectLoadingUnitDetails(DataModels.LoadingUnit l) =>
            new LoadingUnitDetails(l.Id)
            {
                Code = l.Code,
                AbcClassId = l.AbcClassId,
                CellPositionId = l.CellPositionId,
                LoadingUnitStatusId = l.LoadingUnitStatusId,
                LoadingUnitTypeId = l.LoadingUnitTypeId,
                Width = l.LoadingUnitType.LoadingUnitSizeClass.Width,
                Length = l.LoadingUnitType.LoadingUnitSizeClass.Length,
            };

        #endregion Methods
    }
}
