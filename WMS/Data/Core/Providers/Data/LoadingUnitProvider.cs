using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class LoadingUnitProvider : BaseProvider, ILoadingUnitProvider
    {
        #region Fields

        private readonly IMapper mapper;

        #endregion

        #region Constructors

        public LoadingUnitProvider(
            DatabaseContext dataContext,
            IMapper mapper,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.mapper = mapper;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<LoadingUnitDetails>> CreateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var validationError = model.ValidateBusinessModel(this.DataContext.LoadingUnits);
            if (!string.IsNullOrEmpty(validationError))
            {
                return new BadRequestOperationResult<LoadingUnitDetails>(
                    validationError,
                    model);
            }

            var entry = await this.DataContext.LoadingUnits.AddAsync(
                this.mapper.Map<Common.DataModels.LoadingUnit>(model));

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount <= 0)
            {
                return new CreationErrorOperationResult<LoadingUnitDetails>();
            }

            var createdModel = await this.GetByIdAsync(entry.Entity.Id);

            this.NotificationService.PushCreate(createdModel);
            if (createdModel.CellId != null)
            {
                this.NotificationService.PushUpdate(new Cell { Id = createdModel.CellId.Value }, createdModel);
            }

            return new SuccessOperationResult<LoadingUnitDetails>(createdModel);
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

            this.DataContext.LoadingUnits.Remove(new Common.DataModels.LoadingUnit { Id = id });

            var changedEntitiesCount = await this.DataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                this.NotificationService.PushDelete(existingModel);
                if (existingModel.CellId != null)
                {
                    this.NotificationService.PushUpdate(new Cell { Id = existingModel.CellId.Value }, existingModel);
                }

                return new SuccessOperationResult<LoadingUnitDetails>(existingModel);
            }

            return new UnprocessableEntityOperationResult<LoadingUnitDetails>();
        }

        public async Task<IEnumerable<LoadingUnit>> GetAllAllowedByItemIdAsync(
            int id,
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions,
            string where,
            string search)
        {
            var models = await this.GetAllBase()
                .Where(l => this.DataContext.ItemsAreas.Where(
                    ia => ia.ItemId == id)
                            .Select(ia => ia.AreaId)
                            .Contains(l.AreaId))
                .Where(l => l.HasCompartments)
                .ToArrayAsync<LoadingUnit, Common.DataModels.LoadingUnit>(
                    skip,
                    take,
                    orderBySortOptions,
                    where,
                    BuildSearchExpression(search));

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
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
                SetPolicies(model);
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
                SetPolicies(model);
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
                SetPolicies(result);
            }

            return result;
        }

        public async Task<LoadingUnitOperation> GetByIdForExecutionAsync(int id)
        {
            return await this.DataContext.LoadingUnits
                .Select(l => new LoadingUnitOperation
                {
                    Id = l.Id,
                    LastPickDate = l.LastPickDate,
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
                this.DataContext.LoadingUnits,
                this.GetAllBase());
        }

        public async Task<IOperationResult<LoadingUnitOperation>> UpdateAsync(LoadingUnitOperation model)
        {
            var result = await this.UpdateAsync<Common.DataModels.LoadingUnit, LoadingUnitOperation, int>(
                model,
                this.DataContext.LoadingUnits,
                this.DataContext);

            this.NotificationService.PushUpdate(model);

            return result;
        }

        public async Task<IEnumerable<LoadingUnitDetails>> GetAllByMachineIdAsync(int machineId)
        {
            var models = await this.GetAllDetailsBase()
                .Where(l => l.MachineId == machineId)
                .ToArrayAsync();

            foreach (var model in models)
            {
                SetPolicies(model);
            }

            return models;
        }

        public async Task<IOperationResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
            if (model == null || !this.IsValidRelationshipBetweenTypeAisle(model))
            {
                return new BadRequestOperationResult<LoadingUnitDetails>(model);
            }

            var result = await this.UpdateAsync<Common.DataModels.LoadingUnit, LoadingUnitDetails, int>(
                model,
                this.DataContext.LoadingUnits,
                this.DataContext);

            this.NotificationService.PushUpdate(model);
            if (model.CellId != null)
            {
                this.NotificationService.PushUpdate(new Cell { Id = model.CellId.Value }, model);
            }

            return result;
        }

        public async Task<IOperationResult<LoadingUnitDetails>> UpdateMissionsCountAsync(int id)
        {
            var model = await this.GetByIdAsync(id);
            model.MissionsCount++;

            var result = await this.UpdateAsync<Common.DataModels.LoadingUnit, LoadingUnitDetails, int>(
                model,
                this.DataContext.LoadingUnits,
                this.DataContext);

            this.NotificationService.PushUpdate(model);

            return result;
        }

        public async Task<IOperationResult<LoadingUnitOperationalInfoUpdate>> UpdateOperationalInfoAsync(LoadingUnitOperationalInfoUpdate model)
        {
            if (model == null)
            {
                return new BadRequestOperationResult<LoadingUnitOperationalInfoUpdate>(model);
            }

            try
            {
                var existingDataModel = this.DataContext.LoadingUnits.Find(model.Id);

                this.DataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
                await this.DataContext.SaveChangesAsync();

                this.NotificationService.PushUpdate(model);
                this.NotificationService.PushUpdate(new Cell { Id = model.CellId }, model);

                return new SuccessOperationResult<LoadingUnitOperationalInfoUpdate>(null);
            }
            catch (Exception ex)
            {
                return new UnprocessableEntityOperationResult<LoadingUnitOperationalInfoUpdate>(ex);
            }
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

        private static void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy((model as ILoadingUnitUpdatePolicy).ComputeUpdatePolicy());
            model.AddPolicy((model as ILoadingUnitDeletePolicy).ComputeDeletePolicy());
            model.AddPolicy((model as ILoadingUnitWithdrawPolicy).ComputeWithdrawPolicy());
        }

        private IQueryable<LoadingUnit> GetAllBase()
        {
            var loadingUnitsMachines = this.DataContext.LoadingUnits.Join(this.DataContext.Machines, l => l.Cell.Aisle.Id, machine => machine.AisleId, (l, m) => new { l, m }).ToArray();
            return this.DataContext.LoadingUnits
                .Select(l => new LoadingUnit
                {
                    Id = l.Id,
                    Code = l.Code,
                    LoadingUnitTypeDescription = l.LoadingUnitType.Description,
                    LoadingUnitStatusDescription = l.LoadingUnitStatus.Description,
                    AbcClassDescription = l.AbcClass.Description,
                    AreaId = l.Cell.Aisle.AreaId,
                    AreaName = l.Cell.Aisle.Area.Name,
                    AisleName = l.Cell.Aisle.Name,
                    CellFloor = l.Cell.Floor,
                    CellColumn = l.Cell.Column,
                    CellId = l.CellId,
                    CellSide = (Side)l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                    CompartmentsCount = l.Compartments.Count(),
                    HasCompartments = l.LoadingUnitType.HasCompartments,
                    ActiveMissionsCount = l.Missions.Count(
                        m => m.Operations.Any(o =>
                            o.Status != Common.DataModels.MissionOperationStatus.Completed
                            &&
                            o.Status != Common.DataModels.MissionOperationStatus.Incomplete)),
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    AreaFillRate = l.Compartments.Sum(x => x.CompartmentType.Width * x.CompartmentType.Depth)
                        / (l.LoadingUnitType.LoadingUnitSizeClass.Width *
                            l.LoadingUnitType.LoadingUnitSizeClass.Depth),
                    WeightFillRate = loadingUnitsMachines.Any(wl => wl.l.Id == l.Id &&
                                                                    wl.m.TotalMaxWeight.HasValue &&
                                                                    wl.m.TotalMaxWeight.Value > 0) ? loadingUnitsMachines.Select(lm => new
                                                                    {
                                                                        WeightFillRate = (double?)(lm.l.Weight / lm.m.TotalMaxWeight.Value),
                                                                        LoadingUnitId = lm.l.Id,
                                                                    }).FirstOrDefault(wl => wl.LoadingUnitId == l.Id).WeightFillRate : 0,
                });
        }

        private IQueryable<LoadingUnitDetails> GetAllDetailsBase()
        {
            return this.DataContext.LoadingUnits
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
                    Depth = l.LoadingUnitType.LoadingUnitSizeClass.Depth,
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
                    LastPutDate = l.LastPutDate,
                    MissionsCount = l.MissionsCount,
                    CellId = l.CellId,
                    MachineId = l.Cell.Aisle.Machines.FirstOrDefault().Id,
                    AisleId = l.Cell.AisleId,
                    AreaId = l.Cell.Aisle.AreaId,
                    EmptyWeight = l.LoadingUnitType.EmptyWeight,
                    MaxNetWeight = l.LoadingUnitType.LoadingUnitWeightClass.MaxWeight,
                    AreaFillRate = l.Compartments.Sum(cmp => cmp.CompartmentType.Width * cmp.CompartmentType.Depth) /
                        (l.LoadingUnitType.LoadingUnitSizeClass.Width * l.LoadingUnitType.LoadingUnitSizeClass.Depth),
                    CompartmentsCount = l.Compartments.Count(),
                    ActiveSchedulerRequestsCount = l.SchedulerRequests.Count(),
                    ActiveMissionsCount = l.Missions.Count(m =>
                        m.Operations.Any(o =>
                            o.Status != Common.DataModels.MissionOperationStatus.Completed
                            &&
                            o.Status != Common.DataModels.MissionOperationStatus.Incomplete)),
                });
        }

        private IQueryable<LoadingUnitSize> GetSizeInfo(int typeId)
        {
            return this.DataContext.LoadingUnitTypes
                .Where(t => t.Id == typeId)
                .Select(t => new LoadingUnitSize
                {
                    Depth = t.LoadingUnitSizeClass.Depth,
                    Width = t.LoadingUnitSizeClass.Width,
                })
                .Distinct();
        }

        private bool IsValidRelationshipBetweenTypeAisle(LoadingUnitDetails model)
        {
            if (!model.CellId.HasValue)
            {
                return true;
            }

            var existingRelationship =
                this.DataContext.Cells
                    .Where(c => c.Id == model.CellId)
                    .Select(c => new
                    {
                        AisleId = c.AisleId,
                    })
                    .Join(
                        this.DataContext.LoadingUnitTypesAisles,
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
