using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.EF;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal class LoadingUnitProvider : ILoadingUnitProvider
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

        public async Task<OperationResult<LoadingUnitDetails>> CreateAsync(LoadingUnitDetails model)
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
                Reference = (Common.DataModels.ReferenceType)model.ReferenceType,
                Weight = model.Weight,
            });

            var changedEntitiesCount = await this.dataContext.SaveChangesAsync();
            if (changedEntitiesCount > 0)
            {
                model.Id = entry.Entity.Id;
                model.CreationDate = entry.Entity.CreationDate;
                model.LastModificationDate = entry.Entity.LastModificationDate;
                model.LastPickDate = entry.Entity.LastPickDate;
                model.LastStoreDate = entry.Entity.LastStoreDate;
                model.OtherCycleCount = entry.Entity.OtherCycleCount;
                model.OutCycleCount = entry.Entity.OutCycleCount;
            }

            return new SuccessOperationResult<LoadingUnitDetails>(model);
        }

        public async Task<IEnumerable<LoadingUnit>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            Expression<Func<LoadingUnit, bool>> whereExpression = null,
            Expression<Func<LoadingUnit, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression)
                       .ToArrayAsync();
        }

        public async Task<int> GetAllCountAsync(
            Expression<Func<LoadingUnit, bool>> whereExpression = null,
            Expression<Func<LoadingUnit, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ApplyTransform(whereExpression, searchExpression)
                       .CountAsync();
        }

        public async Task<IEnumerable<LoadingUnitDetails>> GetByCellIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                       .Where(l => l.CellId == id)
                       .ToArrayAsync();
        }

        public async Task<LoadingUnitDetails> GetByIdAsync(int id)
        {
            var compartmentsCount =
                await this.dataContext.Compartments
                    .CountAsync(c => c.LoadingUnitId == id);

            var result = await this.GetAllDetailsBase()
                             .SingleOrDefaultAsync(l => l.Id == id);
            result.CompartmentsCount = compartmentsCount;
            return result;
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(propertyName, this.dataContext.LoadingUnits);
        }

        public async Task<OperationResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.LoadingUnits.Find(model.Id);

            if (existingModel == null)
            {
                return new NotFoundOperationResult<LoadingUnitDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<LoadingUnitDetails>(model);
        }

        private IQueryable<LoadingUnit> GetAllBase()
        {
            return this.dataContext.LoadingUnits
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
                    CellSide = (Side)l.Cell.Side,
                    CellNumber = l.Cell.CellNumber,
                    CellPositionDescription = l.CellPosition.Description,
                });
        }

        private IQueryable<LoadingUnitDetails> GetAllDetailsBase()
        {
            return this.dataContext.LoadingUnits
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
                    IsCellPairingFixed = l.IsCellPairingFixed,
                    ReferenceType = (ReferenceType)l.Reference,
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
                });
        }

        #endregion
    }
}
