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
            return new SuccessOperationResult<LoadingUnitDetails>(existingModel);
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

        public async Task<LoadingUnitOperation> GetByIdForExecutionAsync(int id)
        {
            return await this.dataContext.LoadingUnits
               .Select(l => new LoadingUnitOperation
               {
                   Id = l.Id,
                   LastPickDate = l.LastPickDate
               })
               .SingleOrDefaultAsync(l => l.Id == id);
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

        public async Task<IOperationResult<LoadingUnitOperation>> UpdateAsync(LoadingUnitOperation model)
        {
            return await this.UpdateAsync<Common.DataModels.LoadingUnit, LoadingUnitOperation, int>(
                model,
                this.dataContext.LoadingUnits,
                this.dataContext);
        }

        public async Task<IOperationResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
           if (model != null && this.IsValidRelationshipBetweenTypeAisle(model) == false)
            {
                return new BadRequestOperationResult<LoadingUnitDetails>(model);
            }

            return await this.UpdateAsync<Common.DataModels.LoadingUnit, LoadingUnitDetails, int>(
                model,
                this.dataContext.LoadingUnits,
                this.dataContext);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<LoadingUnitDetails, bool>> BuildDetailsSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (l) =>
                (l.AbcClassDescription != null && l.AbcClassDescription.Contains(search))
                || (l.CellPositionDescription != null && l.CellPositionDescription.Contains(search))
                || (l.LoadingUnitStatusDescription != null && l.LoadingUnitStatusDescription.Contains(search))
                || (l.LoadingUnitTypeDescription != null && l.LoadingUnitTypeDescription.Contains(search))
                || (l.CellPositionDescription != null && l.CellPositionDescription.Contains(search));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<LoadingUnit, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsInt = int.TryParse(search, out var searchAsInt);

            return (l) =>
                (l.AreaName != null && l.AreaName.Contains(search))
                || (l.AisleName != null && l.AisleName.Contains(search))
                || (l.Code != null && l.Code.Contains(search))
                || (l.LoadingUnitTypeDescription != null && l.LoadingUnitTypeDescription.Contains(search))
                || (l.LoadingUnitStatusDescription != null && l.LoadingUnitStatusDescription.Contains(search))
                || (l.AbcClassDescription != null && l.AbcClassDescription.Contains(search))
                || (l.CellPositionDescription != null && l.CellPositionDescription.Contains(search))
                || (l.CellSide != null && l.CellSide.ToString().Contains(search))
                || (successConversionAsInt
                    && (Equals(l.CellFloor, searchAsInt)
                        || Equals(l.CellColumn, searchAsInt)
                        || Equals(l.CellNumber, searchAsInt)));
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
                    CellId = l.CellId,
                    CellSide = (Side)l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,

                    CompartmentsCount = l.Compartments.Count(),
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Status != Common.DataModels.MissionStatus.Completed
                            && m.Status != Common.DataModels.MissionStatus.Incomplete),
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    AreaFillRate = l.Compartments.Sum(x => x.CompartmentType.Width * x.CompartmentType.Height)
                        / (l.LoadingUnitType.LoadingUnitSizeClass.Width *
                            l.LoadingUnitType.LoadingUnitSizeClass.Length),
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

        private bool IsValidRelationshipBetweenTypeAisle(LoadingUnitDetails model)
        {
            if (model.CellId.HasValue == false)
            {
                return true;
            }

            var existingRelationship =
                this.dataContext.Cells
                    .Where(c => c.Id == model.CellId)
                    .Select(c => new
                    {
                        AisleId = c.AisleId,
                    })
                    .Join(
                        this.dataContext.LoadingUnitTypesAisles,
                        c => c.AisleId,
                        t => t.AisleId,
                        (c, t) => new
                        {
                            Aisle = t.AisleId,
                            LoadingUnitType = t.LoadingUnitTypeId,
                        })
               .FirstOrDefault(x => x.LoadingUnitType == model.LoadingUnitTypeId);
            return existingRelationship != null;
        }

        #endregion
    }
}
