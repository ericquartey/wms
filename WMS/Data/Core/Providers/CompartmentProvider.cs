using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.EF;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly DatabaseContext dataContext;

        #endregion

        #region Constructors

        public CompartmentProvider(
            DatabaseContext dataContext,
            ICompartmentTypeProvider compartmentTypeProvider)
        {
            this.dataContext = dataContext;
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion

        #region Methods

        public async Task<OperationResult<IEnumerable<CompartmentDetails>>> CreateRangeAsync(
            IEnumerable<CompartmentDetails> compartments)
        {
            if (compartments == null)
            {
                throw new ArgumentNullException(nameof(compartments));
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var compartment in compartments)
                {
                    var result = await this.CreateAsync(compartment);
                    if (!result.Success)
                    {
                        return new CreationErrorOperationResult<IEnumerable<CompartmentDetails>>();
                    }

                    compartment.Id = result.Entity.Id;
                    compartment.CreationDate = result.Entity.CreationDate;
                }

                scope.Complete();
                return new SuccessOperationResult<IEnumerable<CompartmentDetails>>(compartments);
            }
        }

        public async Task<OperationResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var createCompartmentTypeResult = await this.compartmentTypeProvider.CreateAsync(
                                                      new CompartmentType
                                                      {
                                                          Width = model.Width,
                                                          Height = model.Height
                                                      },
                                                      model.ItemId,
                                                      model.MaxCapacity);

                if (!createCompartmentTypeResult.Success)
                {
                    return new CreationErrorOperationResult<CompartmentDetails>();
                }

                var entry = await this.dataContext.Compartments.AddAsync(new Common.DataModels.Compartment
                {
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    CompartmentTypeId = createCompartmentTypeResult.Entity.Id,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    Stock = model.Stock,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    CreationDate = DateTime.Now,
                    ItemId = model.ItemId,
                    MaterialStatusId = model.MaterialStatusId
                });

                if (await this.dataContext.SaveChangesAsync() > 0)
                {
                    model.Id = entry.Entity.Id;
                }

                scope.Complete();
                return new SuccessOperationResult<CompartmentDetails>(model);
            }
        }

        public async Task<OperationResult<CompartmentDetails>> DeleteAsync(int id)
        {
            var existingModel = this.dataContext.Compartments.Find(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CompartmentDetails>();
            }

            this.dataContext.Remove(existingModel);
            await this.dataContext.SaveChangesAsync();
            return new SuccessOperationResult<CompartmentDetails>();
        }

        public async Task<IEnumerable<Compartment>> GetAllAsync(
            int skip,
            int take,
            string orderBy = null,
            IExpression whereExpression = null,
            Expression<Func<Compartment, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .ToArrayAsync(
                           skip,
                           take,
                           orderBy,
                           whereExpression,
                           searchExpression);
        }

        public async Task<int> GetAllCountAsync(
            IExpression whereExpression = null,
            Expression<Func<Compartment, bool>> searchExpression = null)
        {
            return await this.GetAllBase()
                       .CountAsync(whereExpression, searchExpression);
        }

        public async Task<IEnumerable<AllowedItemInCompartment>> GetAllowedItemsAsync(int id)
        {
            return await this.dataContext.Compartments
                .Where(c => c.Id == id)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.AbcClass)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .ThenInclude(ict => ict.Item)
                .ThenInclude(i => i.ItemCategory)
                .SelectMany(
                    c => c.CompartmentType.ItemsCompartmentTypes,
                    (c, ict) => new AllowedItemInCompartment
                    {
                        Id = ict.Item.Id,
                        Code = ict.Item.Code,
                        Description = ict.Item.Description,
                        MaxCapacity = ict.MaxCapacity,
                        AbcClassDescription = ict.Item.AbcClass.Description,
                        AbcClassId = ict.Item.AbcClassId,
                        ItemCategoryId = ict.Item.ItemCategoryId,
                        ItemCategoryDescription = ict.Item.ItemCategory.Description,
                        Image = ict.Item.Image,
                    })
                .ToArrayAsync();
        }

        public async Task<CompartmentDetails> GetByIdAsync(int id)
        {
            var allowedItemsCount =
                await this.dataContext.Compartments
                    .Where(c => c.Id == id)
                    .Include(c => c.CompartmentType)
                    .ThenInclude(ct => ct.ItemsCompartmentTypes)
                    .Include(c => c.CompartmentType)
                    .ThenInclude(ct => ct.ItemsCompartmentTypes)
                    .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                    .CountAsync();

            var result = await this.GetAllDetailsBase()
                       .SingleOrDefaultAsync(c => c.Id == id);
            result.AllowedItemsCount = allowedItemsCount;
            return result;
        }

        public async Task<IEnumerable<Compartment>> GetByItemIdAsync(int id)
        {
            return await this.GetAllBase()
                       .Where(c => c.ItemId == id)
                       .ToArrayAsync();
        }

        public async Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                       .Where(c => c.LoadingUnitId == id)
                       .ToArrayAsync();
        }

        public async Task<int?> GetMaxCapacityAsync(int width, int height, int itemId)
        {
            var compartmentType = await this.dataContext.ItemsCompartmentTypes
                                      .Include(ict => ict.CompartmentType)
                                      .SingleOrDefaultAsync(ict =>
                                                                ict.ItemId == itemId &&
                                                                ((ict.CompartmentType.Width == width &&
                                                                  ict.CompartmentType.Height == height) ||
                                                                 (ict.CompartmentType.Width == height &&
                                                                  ict.CompartmentType.Height == width)));

            return compartmentType?.MaxCapacity;
        }

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(propertyName, this.dataContext.Compartments);
        }

        public async Task<OperationResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Compartments.Find(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CompartmentDetails>();
            }

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);
            await this.dataContext.SaveChangesAsync();

            return new SuccessOperationResult<CompartmentDetails>(model);
        }

        private IQueryable<Compartment> GetAllBase()
        {
            return this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .Include(c => c.MaterialStatus)
                .Include(c => c.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(c => c.CompartmentType)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.PackageType)
                .Select(c => new Compartment
                {
                    Id = c.Id,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    HasRotation = c.HasRotation,
                    Width = c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width,
                    Height = c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height,
                    ItemDescription = c.Item.Description,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    ItemId = c.ItemId,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatus.Description,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    ItemMeasureUnit = c.Item.MeasureUnit.Description,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                    LoadingUnitId = c.LoadingUnitId,
                });
        }

        private IQueryable<CompartmentDetails> GetAllDetailsBase()
        {
            return this.dataContext.Compartments
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.LoadingUnitType)
                .Include(c => c.Item)
                .ThenInclude(i => i.MeasureUnit)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.CompartmentType)
                .GroupJoin(
                    this.dataContext.ItemsCompartmentTypes,
                    cmp => new { CompartmentTypeId = cmp.CompartmentTypeId, ItemId = cmp.ItemId.Value },
                    ict => new { CompartmentTypeId = ict.CompartmentTypeId, ItemId = ict.ItemId },
                    (cmp, ict) => new { cmp, ict = ict.DefaultIfEmpty() })
                .Select(j => new CompartmentDetails
                {
                    Id = j.cmp.Id,
                    LoadingUnitCode = j.cmp.LoadingUnit.Code,
                    CompartmentTypeId = j.cmp.CompartmentTypeId,
                    IsItemPairingFixed = j.cmp.IsItemPairingFixed,
                    ItemCode = j.cmp.Item.Code,
                    ItemDescription = j.cmp.Item.Description,
                    Sub1 = j.cmp.Sub1,
                    Sub2 = j.cmp.Sub2,
                    MaterialStatusId = j.cmp.MaterialStatusId,
                    FifoTime = j.cmp.FifoTime,
                    PackageTypeId = j.cmp.PackageTypeId,
                    Lot = j.cmp.Lot,
                    RegistrationNumber = j.cmp.RegistrationNumber,
                    MaxCapacity = j.ict.SingleOrDefault().MaxCapacity,
                    Stock = j.cmp.Stock,
                    ReservedForPick = j.cmp.ReservedForPick,
                    ReservedToStore = j.cmp.ReservedToStore,
                    CompartmentStatusId = j.cmp.CompartmentStatusId,
                    CompartmentStatusDescription = j.cmp.CompartmentStatus.Description,
                    CreationDate = j.cmp.CreationDate,
                    LastHandlingDate = j.cmp.LastHandlingDate,
                    InventoryDate = j.cmp.InventoryDate,
                    FirstStoreDate = j.cmp.FirstStoreDate,
                    LastStoreDate = j.cmp.LastStoreDate,
                    LastPickDate = j.cmp.LastPickDate,
                    Width = j.cmp.HasRotation ? j.cmp.CompartmentType.Height : j.cmp.CompartmentType.Width,
                    Height = j.cmp.HasRotation ? j.cmp.CompartmentType.Width : j.cmp.CompartmentType.Height,
                    XPosition = j.cmp.XPosition,
                    YPosition = j.cmp.YPosition,
                    LoadingUnitId = j.cmp.LoadingUnitId,
                    ItemId = j.cmp.ItemId,
                    LoadingUnitHasCompartments = j.cmp.LoadingUnit.LoadingUnitType.HasCompartments,
                    ItemMeasureUnit = j.cmp.Item.MeasureUnit.Description
                });
        }

        #endregion
    }
}
