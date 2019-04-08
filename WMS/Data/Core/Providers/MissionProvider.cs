using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
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
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .ToArrayAsync<Mission, Common.DataModels.Mission>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Mission, Common.DataModels.Mission>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<Mission> GetByIdAsync(int id)
        {
            return await this.GetAllBase()
                       .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IOperationResult<MissionDetails>> GetDetailsByIdAsync(int id)
        {
            var missionDetails = await this.dataContext.Missions
                .Where(m => m.Id == id)
                .Select(m => new MissionDetails
                {
                    BayId = m.BayId,
                    CompartmentId = m.CompartmentId,
                    Id = m.Id,
                    ItemId = m.ItemId,
                    Lot = m.Lot,
                    MaterialStatusDescription = m.MaterialStatus.Description,
                    MaterialStatusId = m.MaterialStatusId,
                    PackageTypeDescription = m.PackageType.Description,
                    PackageTypeId = m.PackageTypeId,
                    Priority = m.Priority,
                    RegistrationNumber = m.RegistrationNumber,
                    RequestedQuantity = m.RequestedQuantity,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type,
                    LoadingUnit = new LoadingUnitContentInfo
                    {
                        Id = m.LoadingUnit.Id,
                        Width = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Width,
                        Length = m.LoadingUnit.LoadingUnitType.LoadingUnitSizeClass.Length,
                        Compartments = m.LoadingUnit.Compartments.Select(c => new CompartmentContentInfo
                        {
                            Id = c.Id,
                            Width = c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width,
                            Height = c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height,
                            Stock = c.Stock,
                            MaxCapacity = c.ItemId.HasValue ? c.CompartmentType.ItemsCompartmentTypes.SingleOrDefault(ict => ict.ItemId == c.ItemId).MaxCapacity : null,
                        })
                    },
                    ItemList = new ItemList
                    {
                        Id = m.ItemList.Id,
                        Code = m.ItemList.Code,
                        Description = m.ItemList.Description
                    },
                    ItemListRow = new ItemListRow
                    {
                        Id = m.ItemListRow.Id,
                        Code = m.ItemListRow.Code,

                        // TODO ADD m.ItemListRow.Description????
                    },
                })
                .SingleOrDefaultAsync();

            if (missionDetails == null)
            {
                return new NotFoundOperationResult<MissionDetails>();
            }

            return new SuccessOperationResult<MissionDetails>(missionDetails);
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
                m.BayDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.ItemListDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.ItemListRowCode.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.LoadingUnitCode.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Priority.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.RequestedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Type.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                m.Status.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ;
        }

        private IQueryable<Mission> GetAllBase()
        {
            return this.dataContext.Missions
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
                    DispatchedQuantity = m.DispatchedQuantity,
                    RegistrationNumber = m.RegistrationNumber,
                    RequestedQuantity = m.RequestedQuantity,
                    Status = (MissionStatus)m.Status,
                    Sub1 = m.Sub1,
                    Sub2 = m.Sub2,
                    Type = (MissionType)m.Type,
                });
        }

        #endregion
    }
}
