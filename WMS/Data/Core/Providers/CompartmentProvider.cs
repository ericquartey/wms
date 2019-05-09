using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly DatabaseContext dataContext;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        #endregion

        #region Constructors

        public CompartmentProvider(
            DatabaseContext dataContext,
            ICompartmentTypeProvider compartmentTypeProvider,
            ILoadingUnitProvider loadingUnitProvider)
        {
            this.dataContext = dataContext;
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            if (model == null || model.Height == null || model.Width == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(model.LoadingUnitId);
            var compartmentsDetails = await this.GetByLoadingUnitIdAsync(model.LoadingUnitId);
            var errors = model.CheckCompartment();
            if (string.IsNullOrEmpty(errors) == false)
            {
                return new CreationErrorOperationResult<CompartmentDetails>(errors);
            }

            if (model.CanAddToLoadingUnit(compartmentsDetails, loadingUnit) == false)
            {
                return new CreationErrorOperationResult<CompartmentDetails>(Errors.CompartmentSetCannotBeInsertedInLoadingUnit);
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

        public async Task<IOperationResult<IEnumerable<CompartmentDetails>>> CreateRangeAsync(
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

        public async Task<IOperationResult<CompartmentDetails>> DeleteAsync(int id)
        {
            var existingModel = await this.GetByIdAsync(id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CompartmentDetails>();
            }

            if (!existingModel.CanDelete())
            {
                return new UnprocessableEntityOperationResult<CompartmentDetails>();
            }

            this.dataContext.Remove(new Common.DataModels.Compartment { Id = id });
            await this.dataContext.SaveChangesAsync();
            return new SuccessOperationResult<CompartmentDetails>(existingModel);
        }

        public async Task<IEnumerable<Compartment>> GetAllAsync(
            int skip,
            int take,
            IEnumerable<SortOption> orderBySortOptions = null,
            string whereString = null,
            string searchString = null)
        {
            var models = await this.GetAllBase()
                .ToArrayAsync<Compartment, Common.DataModels.Compartment>(
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

        public async Task<int> GetAllCountAsync(
            string whereString = null,
            string searchString = null)
        {
            return await this.GetAllBase()
                .CountAsync<Compartment, Common.DataModels.Compartment>(
                    whereString,
                    BuildSearchExpression(searchString));
        }

        public async Task<IEnumerable<AllowedItemInCompartment>> GetAllowedItemsAsync(int id)
        {
            return await this.dataContext.Compartments
                .Where(c => c.Id == id)
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
                    .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                    .CountAsync();

            var model = await this.GetAllDetailsBase()
                       .SingleOrDefaultAsync(c => c.Id == id);

            if (model != null)
            {
                model.AllowedItemsCount = allowedItemsCount;
                this.SetPolicies(model);
            }

            return model;
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

        public async Task<double?> GetMaxCapacityAsync(double width, double height, int itemId)
        {
            var compartmentType = await this.dataContext.ItemsCompartmentTypes
                .SingleOrDefaultAsync(ict =>
                    ict.ItemId == itemId &&
                    (((int)ict.CompartmentType.Width == (int)width &&
                            (int)ict.CompartmentType.Height == (int)height) ||
                        ((int)ict.CompartmentType.Width == (int)height &&
                            (int)ict.CompartmentType.Height == (int)width)));

            return compartmentType?.MaxCapacity;
        }

        public async Task<IEnumerable<object>> GetUniqueValuesAsync(string propertyName)
        {
            return await this.GetUniqueValuesAsync(
                       propertyName,
                       this.dataContext.Compartments,
                       this.GetAllBase());
        }

        public async Task<IOperationResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var errors = model.CheckCompartment();
            if (string.IsNullOrEmpty(errors) == false)
            {
                return new CreationErrorOperationResult<CompartmentDetails>(errors);
            }

            var existingModel = await this.GetByIdAsync(model.Id);
            if (existingModel == null)
            {
                return new NotFoundOperationResult<CompartmentDetails>();
            }

            if (!existingModel.CanUpdate())
            {
                return new UnprocessableEntityOperationResult<CompartmentDetails>
                {
                    Description = existingModel.GetCanDeleteReason(),
                };
            }

            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(model.LoadingUnitId);
            var compartmentsDetails = await this.GetByLoadingUnitIdAsync(model.LoadingUnitId);
            if (model.CanAddToLoadingUnit(compartmentsDetails, loadingUnit) == false)
            {
                return new CreationErrorOperationResult<CompartmentDetails>(Errors.CompartmentSetCannotBeInsertedInLoadingUnit);
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

                var existingDataModel = this.dataContext.Compartments.Find(model.Id);
                model.CompartmentTypeId = createCompartmentTypeResult.Entity.Id;
                this.dataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
                await this.dataContext.SaveChangesAsync();

                scope.Complete();
                return new SuccessOperationResult<CompartmentDetails>(model);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Major Code Smell",
            "S4058:Overloads with a \"StringComparison\" parameter should be used",
            Justification = "StringComparison inhibit translation of lambda expression to SQL query")]
        private static Expression<Func<Compartment, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            var successConversionAsDouble = double.TryParse(search, out var searchAsDouble);

            return (c) =>
                (c.CompartmentStatusDescription != null && c.CompartmentStatusDescription.Contains(search))
                || (c.ItemDescription != null && c.ItemDescription.Contains(search))
                || (c.ItemMeasureUnit != null && c.ItemMeasureUnit.Contains(search))
                || (c.LoadingUnitCode != null && c.LoadingUnitCode.Contains(search))
                || (c.Lot != null && c.Lot.Contains(search))
                || (c.MaterialStatusDescription != null && c.MaterialStatusDescription.Contains(search))
                || (c.Sub1 != null && c.Sub1.Contains(search))
                || (c.Sub2 != null && c.Sub2.Contains(search))
                || (successConversionAsDouble
                    && Equals(c.Stock, searchAsDouble));
        }

        private IQueryable<Compartment> GetAllBase()
        {
            return this.dataContext.Compartments
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
                    ItemMeasureUnit = j.cmp.Item.MeasureUnit.Description,
                });
        }

        #endregion
    }
}
