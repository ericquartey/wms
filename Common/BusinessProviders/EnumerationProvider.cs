using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class EnumerationProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public EnumerationProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public static IEnumerable<Enumeration> GetAllItemManagementTypes()
        {
            var values = System.Enum.GetValues(typeof(ItemManagementType));

            return values.Cast<ItemManagementType>()
                .Select(x => new Enumeration((int)x, System.Enum.GetName(typeof(ItemManagementType), x)));
        }

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
                    $"{a.Area.Name} - {a.Name}"));
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
                .OrderBy(x => x.Width * x.Height)
                .Select(x => new Enumeration(x.Id, string.Format(Resources.MasterData.CompartmentTypeListFormat, x.Width, x.Height)));
        }

        public IQueryable<Enumeration> GetAllItemCategories()
        {
            return this.dataContext.ItemCategories
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

        #endregion
    }
}
