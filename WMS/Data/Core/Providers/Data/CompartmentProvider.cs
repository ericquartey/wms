using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.EF;
using Ferretto.Common.Resources;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Core.Policies;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.WMS.Data.Core.Providers
{
    internal partial class CompartmentProvider : BaseProvider, ICompartmentProvider
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        #endregion

        #region Constructors

        public CompartmentProvider(
            DatabaseContext dataContext,
            ICompartmentTypeProvider compartmentTypeProvider,
            ILoadingUnitProvider loadingUnitProvider,
            INotificationService notificationService)
            : base(dataContext, notificationService)
        {
            this.compartmentTypeProvider = compartmentTypeProvider;
            this.loadingUnitProvider = loadingUnitProvider;
        }

        #endregion

        #region Methods

        public async Task<IOperationResult<CompartmentDetails>> CreateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.Height == null
                || model.Width == null
                || !model.XPosition.HasValue
                || !model.YPosition.HasValue)
            {
                return new CreationErrorOperationResult<CompartmentDetails>(
                    "Compartment position and size must be specified.");
            }

            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(model.LoadingUnitId);
            var existingCompartents = await this.GetByLoadingUnitIdAsync(model.LoadingUnitId);
            var errors = model.GetValidationMessages();
            if (!string.IsNullOrEmpty(errors))
            {
                return new CreationErrorOperationResult<CompartmentDetails>(errors);
            }

            if (!model.CanAddToLoadingUnit(existingCompartents, loadingUnit))
            {
                return new CreationErrorOperationResult<CompartmentDetails>(Errors
                    .CompartmentSetCannotBeInsertedInLoadingUnit);
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
                    return new CreationErrorOperationResult<CompartmentDetails>(
                        createCompartmentTypeResult.Description);
                }

                var filteredModel = CleanCompartmentItemDetails(model);
                var compartment = new Common.DataModels.Compartment
                {
                    XPosition = filteredModel.XPosition.Value,
                    YPosition = filteredModel.YPosition.Value,
                    LoadingUnitId = filteredModel.LoadingUnitId,
                    CompartmentTypeId = createCompartmentTypeResult.Entity.Id,
                    IsItemPairingFixed = filteredModel.IsItemPairingFixed,
                    Stock = filteredModel.Stock,
                    ReservedForPick = filteredModel.ReservedForPick,
                    ReservedToPut = filteredModel.ReservedToPut,
                    ItemId = filteredModel.ItemId,
                    MaterialStatusId = filteredModel.MaterialStatusId,
                    Sub1 = filteredModel.Sub1,
                    Sub2 = filteredModel.Sub2,
                    PackageTypeId = filteredModel.PackageTypeId,
                    Lot = filteredModel.Lot,
                    RegistrationNumber = filteredModel.RegistrationNumber,
                };

                var entry = await this.DataContext.Compartments.AddAsync(compartment);
                if (await this.DataContext.SaveChangesAsync() > 0)
                {
                    model.Id = entry.Entity.Id;
                }

                this.NotificationService.PushCreate(model);

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
                        return new CreationErrorOperationResult<IEnumerable<CompartmentDetails>>(result.Description);
                    }

                    compartment.Id = result.Entity.Id;
                    compartment.CreationDate = result.Entity.CreationDate;

                    this.NotificationService.PushCreate(compartment);
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

            this.DataContext.Remove(new Common.DataModels.Compartment { Id = id });
            await this.DataContext.SaveChangesAsync();

            this.NotificationService.PushDelete(existingModel);

            return new SuccessOperationResult<CompartmentDetails>(existingModel);
        }

        public async Task<IEnumerable<AllowedItemInCompartment>> GetAllowedItemsAsync(int id)
        {
            return await this.DataContext.Compartments
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

        public async Task<IEnumerable<CompartmentDetails>> GetByLoadingUnitIdAsync(int id)
        {
            return await this.GetAllDetailsBase()
                .Where(c => c.LoadingUnitId == id)
                .ToArrayAsync();
        }

        public async Task<double?> GetMaxCapacityAsync(double width, double height, int itemId)
        {
            var compartmentType = await this.DataContext.ItemsCompartmentTypes
                .SingleOrDefaultAsync(ict =>
                    ict.ItemId == itemId &&
                    (((int)ict.CompartmentType.Width == (int)width &&
                            (int)ict.CompartmentType.Height == (int)height) ||
                        ((int)ict.CompartmentType.Width == (int)height &&
                            (int)ict.CompartmentType.Height == (int)width)));

            return compartmentType?.MaxCapacity;
        }

        public async Task<IOperationResult<CompartmentDetails>> UpdateAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var errors = model.GetValidationMessages();
            if (!string.IsNullOrEmpty(errors))
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
                    Description = existingModel.GetCanUpdateReason(),
                };
            }

            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(model.LoadingUnitId);
            var compartmentsDetails = await this.GetByLoadingUnitIdAsync(model.LoadingUnitId);
            if (!model.CanAddToLoadingUnit(compartmentsDetails, loadingUnit))
            {
                return new CreationErrorOperationResult<CompartmentDetails>(Errors
                    .CompartmentSetCannotBeInsertedInLoadingUnit);
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

                var existingDataModel = this.DataContext.Compartments.Find(model.Id);
                model.CompartmentTypeId = createCompartmentTypeResult.Entity.Id;
                model = CleanCompartmentItemDetails(model);
                this.DataContext.Entry(existingDataModel).CurrentValues.SetValues(model);
                await this.DataContext.SaveChangesAsync();

                this.NotificationService.PushUpdate(model);

                scope.Complete();
                return new SuccessOperationResult<CompartmentDetails>(model);
            }
        }

        private static TModel CleanCompartmentItemDetails<TModel>(TModel model)
                        where TModel : ICompartmentItemDetails
        {
            if (model.Stock.Equals(0))
            {
                model.MaterialStatusId = null;
                model.Sub1 = null;
                model.Sub2 = null;
                model.PackageTypeId = null;
                model.Lot = null;
                model.RegistrationNumber = null;
            }

            return model;
        }

        private static void SetPolicies(BaseModel<int> model)
        {
            model.AddPolicy((model as ICompartmentUpdatePolicy).ComputeUpdatePolicy());
            model.AddPolicy((model as ICompartmentDeletePolicy).ComputeDeletePolicy());
        }

        private IQueryable<CompartmentDetails> GetAllDetailsBase()
        {
            var compartmentsWithMaxCapacity = this.DataContext.Compartments
               .GroupJoin(
                   this.DataContext.ItemsCompartmentTypes,
                   cmp => new { CompartmentTypeId = cmp.CompartmentTypeId, ItemId = cmp.ItemId },
                   ict => new { CompartmentTypeId = ict.CompartmentTypeId, ItemId = (int?)ict.ItemId },
                   (cmp, ict) => new { cmp, MaxCapacity = ict.SingleOrDefault().MaxCapacity });

            return compartmentsWithMaxCapacity
                .Select(j => new CompartmentDetails
                {
                    AisleName = j.cmp.LoadingUnit.Cell.Aisle.Name,
                    AreaName = j.cmp.LoadingUnit.Cell.Aisle.Area.Name,
                    CompartmentStatusDescription = j.cmp.CompartmentStatus.Description,
                    CompartmentStatusId = j.cmp.CompartmentStatusId,
                    CompartmentTypeId = j.cmp.CompartmentTypeId,
                    CreationDate = j.cmp.CreationDate,
                    FifoStartDate = j.cmp.FifoStartDate,
                    Height = j.cmp.HasRotation ? j.cmp.CompartmentType.Width : j.cmp.CompartmentType.Height,
                    Id = j.cmp.Id,
                    InventoryDate = j.cmp.InventoryDate,
                    IsItemPairingFixed = j.cmp.IsItemPairingFixed,
                    ItemCode = j.cmp.Item.Code,
                    ItemDescription = j.cmp.Item.Description,
                    ItemId = j.cmp.ItemId,
                    ItemMeasureUnit = j.cmp.Item.MeasureUnit.Description,
                    LastPickDate = j.cmp.LastPickDate,
                    LastPutDate = j.cmp.LastPutDate,
                    LoadingUnitCode = j.cmp.LoadingUnit.Code,
                    LoadingUnitHasCompartments = j.cmp.LoadingUnit.LoadingUnitType.HasCompartments,
                    LoadingUnitId = j.cmp.LoadingUnitId,
                    Lot = j.cmp.Lot,
                    MaterialStatusId = j.cmp.MaterialStatusId,
                    MaxCapacity = j.MaxCapacity,
                    PackageTypeId = j.cmp.PackageTypeId,
                    RegistrationNumber = j.cmp.RegistrationNumber,
                    ReservedForPick = j.cmp.ReservedForPick,
                    ReservedToPut = j.cmp.ReservedToPut,
                    Stock = j.cmp.Stock,
                    Sub1 = j.cmp.Sub1,
                    Sub2 = j.cmp.Sub2,
                    Width = j.cmp.HasRotation ? j.cmp.CompartmentType.Height : j.cmp.CompartmentType.Width,
                    XPosition = j.cmp.XPosition,
                    YPosition = j.cmp.YPosition,
                });
        }

        #endregion
    }
}
