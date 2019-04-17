using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class LoadingUnitProvider : ILoadingUnitProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public LoadingUnitProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<LoadingUnitCreating>> CreateAsync(LoadingUnitCreating model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var entry = await this.dataContext.LoadingUnits.AddAsync(new Common.DataModels.LoadingUnit
            {
                AbcClassId = model.AbcClassId,
                CellId = model.CellId,
                CellPositionId = model.CellPositionId,
                Code = model.Code,
                HandlingParametersCorrection = model.HandlingParametersCorrection,
                Height = model.Height,
                InCycleCount = model.InCycleCount,
                IsCellPairingFixed = model.IsCellPairingFixed,
                LoadingUnitStatusId = model.LoadingUnitStatusId,
                LoadingUnitTypeId = model.LoadingUnitTypeId,
                Note = model.Note,
                ReferenceType = (Common.DataModels.ReferenceType)model.ReferenceType,
                Weight = model.Weight,
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.OtherCycleCount = entry.Entity.OtherCycleCount;
                model.OutCycleCount = entry.Entity.OutCycleCount;
            }

            return new SuccessOperationResult<LoadingUnitCreating>(model);
        }

        public async Task<IOperationResult<LoadingUnitDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<LoadingUnitDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<LoadingUnitDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            this.dataContext.LoadingUnits.Remove(new Common.DataModels.LoadingUnit { Id = id });
            await this.dataContext.SaveChangesAsync();
            return new SuccessOperationResult<LoadingUnitDetails>();
        }

        public async Task<IEnumerable<LoadingUnit>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<LoadingUnit, Common.DataModels.LoadingUnit>(
                    skip,
                    take,
                    orderBySortOptions,
                    whereString,
                    BuildSearchExpression(searchString));

            foreach (var model in models)
            {
                this.SetPolicies(model);
            }

            return models;
        }

        public async Task<IEnumerable<LoadingUnitDetails>> GetAllByCellIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                .Where(l => l.CellId == id)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<LoadingUnitDetails>> GetAllByIdAisleAsync(
            int id,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions,
            string where,
            string search)
        {
            var models = await this.GetAllDetailsBase()
               .Where(l => l.AisleId == id)
               .ToArrayAsync<LoadingUnitDetails, Common.DataModels.LoadingUnit>(
                   skip,
                   take,
                   orderBySortOptions,
                   where,
                   BuildDetailsSearchExpression(search));

            foreach (var model in models)
            {
                this.SetPolicies(model);
            }

            return models;
        }

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<LoadingUnit, Common.DataModels.LoadingUnit>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<LoadingUnitDetails> GetByIdAsync(int id)
        {
            var result = await this.GetAllDetailsBase()
                .SingleOrDefaultAsync(l => l.Id == id);

            if (result != null)
            {
                this.SetPolicies(result);
            }

            return result;
        }

        public async Task<LoadingUnitSize> GetSizeByTypeIdAsync(int typeId)
        {
            return await this.GetSizeInfo(typeId).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                propertyName,
                this.dataContext.LoadingUnits,
                this.GetAllBase());
        }

        public async Task<IOperationResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<LoadingUnitDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<LoadingUnitDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var existingDataModel = this.dataContext.LoadingUnits.Find(model.Id);
            this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<LoadingUnitDetails>(model);
        }

        private static Expression<Func<LoadingUnitDetails, bool>> BuildDetailsSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (l) =>
                l.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellPositionDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.LoadingUnitStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.LoadingUnitTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellPositionDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private static Expression<Func<LoadingUnit, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (l) =>
                l.AbcClassDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.AisleName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.AreaName.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellPositionDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.LoadingUnitStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.LoadingUnitTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellColumn.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellFloor.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                l.CellNumber.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        private IQueryable<LoadingUnit> GetAllBase()
        {
            return this.dataContext.LoadingUnits
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

                    CompartmentsCount = l.Compartments.Count(),
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Status != Common.DataModels.MissionStatus.Completed
                            && m.Status != Common.DataModels.MissionStatus.Incomplete),
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    AreaFillRate = l.Compartments.Sum(x => x.CompartmentType.Width * x.CompartmentType.Height).GetValueOrDefault()
                        / (l.LoadingUnitType.LoadingUnitSizeClass.Width * l.LoadingUnitType.LoadingUnitSizeClass.Length),
                });
        }

        private IQueryable<LoadingUnitDetails> GetAllDetailsBase()
        {
            return this.dataContext.LoadingUnits
                .Select(l => new LoadingUnitDetails
                {
                    Id = l.Id,
                    Code = l.Code,
                    AreaName = l.Cell.Aisle.Area.Name,
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
                    ReferenceType = (ReferenceType)l.ReferenceType,
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
                    EmptyWeight = l.LoadingUnitType.EmptyWeight,
                    MaxNetWeight = l.LoadingUnitType.LoadingUnitWeightClass.MaxWeight,
                    AreaFillRate = l.Compartments.Sum(cmp => cmp.CompartmentType.Width * cmp.CompartmentType.Height) /
                                     (l.LoadingUnitType.LoadingUnitSizeClass.Width * l.LoadingUnitType.LoadingUnitSizeClass.Length),
                    CompartmentsCount = l.Compartments.Count(),
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Status != Common.DataModels.MissionStatus.Completed
                            && m.Status != Common.DataModels.MissionStatus.Incomplete),
                });
        }

        private IQueryable<LoadingUnitSize> GetSizeInfo(int typeId)
        {
            return this.dataContext.LoadingUnitTypes
                .Where(t => t.Id == typeId)
                .Select(t => new LoadingUnitSize
                {
                    Length = t.LoadingUnitSizeClass.Length,
                    Width = t.LoadingUnitSizeClass.Width,
                })
                .Distinct();
        }

        #endregion
    }
}
