using System;
using System.Collections.Generic;
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

        public IQueryable<Enumeration<string>> GetAllAbcClasses()
        {
            return this.dataContext.AbcClasses.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllCellPositions()
        {
            return this.dataContext.CellPositions.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllCompartmentStatuses()
        {
            return this.dataContext.CompartmentStatuses.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllCompartmentTypes()
        {
            return this.dataContext.CompartmentTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllItemCategories()
        {
            return this.dataContext.ItemCategories.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllItemManagementTypes()
        {
            return this.dataContext.ItemManagementTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<string>> GetAllLoadingUnitStatuses()
        {
            return this.dataContext.LoadingUnitStatuses.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllLoadingUnitTypes()
        {
            return this.dataContext.LoadingUnitTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllMaterialStatuses()
        {
            return this.dataContext.MaterialStatuses.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<string>> GetAllMeasureUnits()
        {
            return this.dataContext.MeasureUnits.AsNoTracking().Select(x => new Enumeration<string>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetAllPackageTypes()
        {
            return this.dataContext.PackageTypes.AsNoTracking().Select(x => new Enumeration<int>(x.Id, x.Description));
        }

        public IQueryable<Enumeration<int>> GetCellsByAisleId(int aisleId)
        {
            return this.dataContext.Cells
                .AsNoTracking()
                .Include(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c => c.AisleId == aisleId)
                .OrderBy(c => c.CellNumber)
                .Select(c => new Enumeration<int>(
                    c.Id,
                    $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})")
                );
        }

        public IQueryable<Enumeration<int>> GetCellsByAreaId(int areaId)
        {
            return this.dataContext.Cells
                .AsNoTracking()
                .Include(c => c.Aisle)
                .ThenInclude(a => a.Area)
                .Where(c => c.Aisle.AreaId == areaId)
                .OrderBy(c => c.Aisle.Name)
                .ThenBy(c => c.CellNumber)
                .Select(c => new Enumeration<int>(
                    c.Id,
                    $"{c.Aisle.Area.Name} - {c.Aisle.Name} - Cell {c.CellNumber} (Floor {c.Floor}, Column {c.Column}, {c.Side})")
                );
        }

        #endregion Methods
    }
}
