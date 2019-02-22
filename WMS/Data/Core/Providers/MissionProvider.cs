using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class MissionProvider : IMissionProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public MissionProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Mission>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBy = null,
            string whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync(
                    skip,
                    take,
                    orderBy,
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereExpression = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync(
                    whereExpression,
                    BuildSearchExpression(searchString));
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                       .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Missions,
                       this.GetAllBase());
        }

        private static Expression<Func<Mission, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (m) =>
                m.Lot.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.RegistrationNumber.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Sub1.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Sub2.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Quantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<Mission> GetAllBase()
        {
            return this.dataContext.Missions
                .Include(m => m.Bay)
                .Include(m => m.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(m => m.ItemList)
                .Include(m => m.ItemListRow)
                .Include(m => m.LoadingUnit)
                .Include(m => m.Cell)
                .ThenInclude(c => c.Aisle)
                .Include(m => m.MaterialStatus)
                .Include(m => m.PackageType)
                .Include(m => m.Compartment)
                .ThenInclude(c => c.CompartmentType)
                .Select(m => new Mission
                {
                    BayDescription = m.Bay.Description,
                    BayId = m.BayId,
                    CellAisleName = m.Cell.Aisle.Name,
                    CellId = m.CellId,
                    CompartmentId = m.CompartmentId,
                    CompartmentTypeWidth = m.Compartment.CompartmentType.Width,
                    CompartmentTypeHeight = m.Compartment.CompartmentType.Height,
                    CreationDate = m.CreationDate,
                    Id = m.Id,
                    ItemDescription = m.Item.Description,
                    ItemId = m.ItemId,
                    ItemListDescription = m.ItemList.Description,
                    ItemListId = m.ItemListId,
                    ItemListRowCode = m.ItemListRow.Code,
                    ItemListRowId = m.ItemListRowId,
                    ItemMeasureUnitDescription = m.Item.MeasureUnit.Description,
                    LastModificationDate = m.LastModificationDate,
                    LoadingUnitCode = m.LoadingUnit.Code,
                    LoadingUnitId = m.LoadingUnitId,
                    Lot = m.Lot,
                    MaterialStatusDescription = m.MaterialStatus.Description,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeDescription = m.PackageType.Description,
                    PackageTypeId = m.PackageTypeId,
                    Priority = m.Priority,
                    Quantity = m.RequiredQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    RequiredQuantity = m.RequiredQuantity,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type,
                });
        }

        #endregion
    }
}
