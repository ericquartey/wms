using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class EnumerationProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion Fields

        #region Constructors

        public EnumerationProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion Constructors

        #region Methods

        public IQueryable<Enumeration> GetAislesByAreaId(int areaId)
        {
            return this.dataContext.Aisles
                .AsNoTracking()
                .Include(a => a.Area)
                .Where(a => a.AreaId == areaId)
                .OrderBy(a => a.Area.Name)
                .ThenBy(a => a.Name)
                .Select(a => new Enumeration(
                    a.Id,
                    $"{a.Area.Name} - {a.Name}")
                );
        }

        public IQueryable<EnumerationString> GetAllAbcClasses()
        {
            return this.dataContext.AbcClasses
                .AsNoTracking()
                .Select(x => new EnumerationString(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllCellPositions()
        {
            return this.dataContext.CellPositions
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllCellStatuses()
        {
            return this.dataContext.CellStatuses
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllCellTypes()
        {
            return this.dataContext.CellTypes
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllCompartmentStatuses()
        {
            return this.dataContext.CompartmentStatuses
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllCompartmentTypes()
        {
            return this.dataContext.CompartmentTypes
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllItemCategories()
        {
            return this.dataContext.ItemCategories
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllItemManagementTypes()
        {
            return this.dataContext.ItemManagementTypes
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<EnumerationString> GetAllLoadingUnitStatuses()
        {
            return this.dataContext.LoadingUnitStatuses
                .AsNoTracking()
                .Select(x => new EnumerationString(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllLoadingUnitTypes()
        {
            return this.dataContext.LoadingUnitTypes
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllMaterialStatuses()
        {
            return this.dataContext.MaterialStatuses
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        public IQueryable<EnumerationString> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits
                .AsNoTracking()
                .Select(x => new EnumerationString(x.Id, x.Description));
        }

        public IQueryable<Enumeration> GetAllPackageTypes()
        {
            return this.dataContext.PackageTypes
                .AsNoTracking()
                .Select(x => new Enumeration(x.Id, x.Description));
        }

        #endregion Methods
    }
}
