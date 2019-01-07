using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private static readonly Expression<Func<DataModels.Compartment, bool>> StatusAvailableFilter =
           compartment => compartment.MaterialStatusId == 1;

        private static readonly Expression<Func<DataModels.Compartment, bool>> StatusAwaitingFilter =
           compartment => compartment.MaterialStatusId == 2;

        private static readonly Expression<Func<DataModels.Compartment, bool>> StatusBlockedFilter =
           compartment => compartment.MaterialStatusId == 4;

        private static readonly Expression<Func<DataModels.Compartment, bool>> StatusExpiredFilter =
           compartment => compartment.MaterialStatusId == 3;

        private readonly CompartmentTypeProvider compartmentTypeProvider;

        private readonly IDatabaseContextService dataContextService;

        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CompartmentProvider(
            IDatabaseContextService dataContextService,
            EnumerationProvider enumerationProvider,
            CompartmentTypeProvider compartmentTypeProvider)
        {
            this.dataContextService = dataContextService;
            this.enumerationProvider = enumerationProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion Constructors

        #region Methods

        public async Task<OperationResult> AddAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            // TODO: add transaction management

            try
            {
                var dataContext = this.dataContextService.Current;

                var result = await this.compartmentTypeProvider.AddAsync(new CompartmentType
                {
                    Width = model.Width,
                    Height = model.Height
                }, model.ItemId, model.MaxCapacity);

                if (result.Success == false)
                {
                    return result;
                }

                var entry = dataContext.Compartments.Add(new DataModels.Compartment
                {
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    CompartmentTypeId = result.EntityId.Value,
                    IsItemPairingFixed = model.IsItemPairingFixed,
                    Stock = model.Stock,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    CreationDate = DateTime.Now,
                    ItemId = model.ItemId,
                    MaterialStatusId = model.MaterialStatusId
                });

                var changedEntitiesCount = await dataContext.SaveChangesAsync();
                if (changedEntitiesCount > 0)
                {
                    model.Id = entry.Entity.Id;
                }

                model.LoadingUnit?.Compartments.Add(model);

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public async Task<OperationResult> AddRange(IEnumerable<ICompartment> compartments)
        {
            if (compartments == null)
            {
                throw new ArgumentNullException(nameof(compartments));
            }

            // TODO: add transaction management
            foreach (var compartment in compartments.Cast<CompartmentDetails>())
            {
                var result = await this.AddAsync(compartment);
                if (result.Success == false)
                {
                    return result;
                }
            }

            return new OperationResult(true);
        }

        public int Delete(int id)
        {
            var task = this.DeleteAsync(id);
            task.RunSynchronously();

            return task.Result;
        }

        public async Task<int> DeleteAsync(int id)
        {
            var dataContext = this.dataContextService.Current;

            var existingModel = dataContext.Compartments.Find(id);
            if (existingModel != null)
            {
                dataContext.Remove(existingModel);
            }
            return await dataContext.SaveChangesAsync();
        }

        public IQueryable<Compartment> GetAll()
        {
            return this.dataContextService.Current.Compartments
               .Include(c => c.LoadingUnit)
               .Include(c => c.MaterialStatus)
               .Include(c => c.Item)
               .Include(c => c.CompartmentType)
               .Include(c => c.CompartmentStatus)
               .Include(c => c.PackageType)
               .Select(c => new Compartment
               {
                   Id = c.Id,
                   CompartmentStatusDescription = c.CompartmentStatus.Description,
                   CompartmentTypeDescription = string.Format(
                       Resources.MasterData.CompartmentTypeListFormatReduced,
                       c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height,
                       c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width),
                   ItemDescription = c.Item.Description,
                   IsItemPairingFixed = c.IsItemPairingFixed,
                   LoadingUnitCode = c.LoadingUnit.Code,
                   Lot = c.Lot,
                   MaterialStatusDescription = c.MaterialStatus.Description,
                   Stock = c.Stock,
                   Sub1 = c.Sub1,
                   Sub2 = c.Sub2
               }
               )
               .AsNoTracking();
        }

        public int GetAllCount()
        {
            return this.dataContextService.Current.Compartments.Count();
        }

        public async Task<CompartmentDetails> GetById(int id)
        {
            var dataContext = this.dataContextService.Current;

            var compartmentDetails = await dataContext.Compartments
               .Where(c => c.Id == id)
               .Include(c => c.LoadingUnit)
               .ThenInclude(l => l.LoadingUnitType)
               .Include(c => c.Item)
               .Include(c => c.CompartmentStatus)
               .Include(c => c.CompartmentType)
               .GroupJoin(
                    dataContext.ItemsCompartmentTypes,
                    cmp => new { CompartmentTypeId = cmp.CompartmentTypeId, ItemId = cmp.ItemId.Value },
                    ict => new { CompartmentTypeId = ict.CompartmentTypeId, ItemId = ict.ItemId },
                    (cmp, ict) => new { cmp, ict = ict.DefaultIfEmpty() }
                )
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
                   LoadingUnitHasCompartments = j.cmp.LoadingUnit.LoadingUnitType.HasCompartments
               })
               .SingleAsync();

            compartmentDetails.CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses();
            compartmentDetails.CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes();
            compartmentDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
            compartmentDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();

            return compartmentDetails;
        }

        public IQueryable<Compartment> GetByItemId(int id)
        {
            return this.dataContextService.Current.Compartments
                .Where(c => c.ItemId == id)
                .Include(c => c.LoadingUnit)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.CompartmentType)
                .Include(c => c.Item)
                .Include(c => c.MaterialStatus)
                .Select(c => new Compartment
                {
                    Id = c.Id,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    ItemDescription = c.Item.Description,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    Lot = c.Lot,
                    MaterialStatusDescription = c.MaterialStatus.Description,
                    Stock = c.Stock,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    IsItemPairingFixed = c.IsItemPairingFixed
                })
                .AsNoTracking();
        }

        public IQueryable<CompartmentDetails> GetByLoadingUnitId(int id)
        {
            return this.dataContextService.Current.Compartments
                .Where(c => c.LoadingUnitId == id)
                .Include(c => c.LoadingUnit)
                .ThenInclude(l => l.LoadingUnitType)
                .Include(c => c.Item)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .Select(c => new CompartmentDetails
                {
                    Id = c.Id,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    CompartmentTypeId = c.CompartmentTypeId,
                    ItemCode = c.Item.Code,
                    ItemDescription = c.Item.Description,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    MaterialStatusId = c.MaterialStatusId,
                    FifoTime = c.FifoTime,
                    PackageTypeId = c.PackageTypeId,
                    Lot = c.Lot,
                    RegistrationNumber = c.RegistrationNumber,
                    MaxCapacity = c.CompartmentType.ItemsCompartmentTypes.SingleOrDefault(ict => ict.ItemId == c.ItemId).MaxCapacity,
                    Stock = c.Stock,
                    ReservedForPick = c.ReservedForPick,
                    ReservedToStore = c.ReservedToStore,
                    CompartmentStatusId = c.CompartmentStatusId,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    CreationDate = c.CreationDate,
                    LastHandlingDate = c.LastHandlingDate,
                    InventoryDate = c.InventoryDate,
                    FirstStoreDate = c.FirstStoreDate,
                    LastStoreDate = c.LastStoreDate,
                    LastPickDate = c.LastPickDate,
                    Width = c.HasRotation ? c.CompartmentType.Height : c.CompartmentType.Width,
                    Height = c.HasRotation ? c.CompartmentType.Width : c.CompartmentType.Height,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                    LoadingUnitId = c.LoadingUnitId,
                    ItemId = c.ItemId,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                    LoadingUnitHasCompartments = c.LoadingUnit.LoadingUnitType.HasCompartments
                })
                .AsNoTracking();
        }

        public CompartmentDetails GetNew()
        {
            return new CompartmentDetails
            {
                CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses(),
                CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes(),
                MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses(),
                PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes()
            };
        }

        public IQueryable<Compartment> GetWithStatusAvailable()
        {
            return GetAllCompartmentsWithAggregations(this.dataContextService.Current, StatusAvailableFilter);
        }

        public int GetWithStatusAvailableCount()
        {
            return this.dataContextService.Current.Compartments.AsNoTracking().Count(StatusAvailableFilter);
        }

        public IQueryable<Compartment> GetWithStatusAwaiting()
        {
            return GetAllCompartmentsWithAggregations(this.dataContextService.Current, StatusAwaitingFilter);
        }

        public int GetWithStatusAwaitingCount()
        {
            return this.dataContextService.Current.Compartments.AsNoTracking().Count(StatusAwaitingFilter);
        }

        public IQueryable<Compartment> GetWithStatusBlocked()
        {
            return GetAllCompartmentsWithAggregations(this.dataContextService.Current, StatusBlockedFilter);
        }

        public int GetWithStatusBlockedCount()
        {
            return this.dataContextService.Current.Compartments.AsNoTracking().Count(StatusBlockedFilter);
        }

        public IQueryable<Compartment> GetWithStatusExpired()
        {
            return GetAllCompartmentsWithAggregations(this.dataContextService.Current, StatusExpiredFilter);
        }

        public int GetWithStatusExpiredCount()
        {
            return this.dataContextService.Current.Compartments.AsNoTracking().Count(StatusExpiredFilter);
        }

        public bool HasAnyAllowedItem(int modelId)
        {
            var dataContext = this.dataContextService.Current;

            return dataContext.Compartments
                .Where(c => c.Id == modelId)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                .AsNoTracking()
                .Any();
        }

        public async Task<OperationResult> SaveAsync(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            try
            {
                var result = await this.compartmentTypeProvider.AddAsync(new CompartmentType
                {
                    Width = model.Width,
                    Height = model.Height
                }, model.ItemId, model.MaxCapacity);

                if (result.Success == false)
                {
                    return result;
                }

                using (var dataContext = this.dataContextService.Current)
                {
                    var compartmentType = dataContext.CompartmentTypes.Find(result.EntityId);
                    var existingModel = dataContext.Compartments.Find(model.Id);

                    dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                    existingModel.HasRotation =
                        compartmentType.Height == model.Width
                        &&
                        compartmentType.Width == model.Height;

                    await dataContext.SaveChangesAsync();

                    return new OperationResult(true);
                }
            }
            catch (Exception ex)
            {
                return new OperationResult(ex);
            }
        }

        private static IQueryable<Compartment> GetAllCompartmentsWithAggregations(DatabaseContext context, Expression<Func<DataModels.Compartment, bool>> whereFunc = null)
        {
            var actualWhereFunc = whereFunc ?? ((i) => true);

            return context.Compartments
               .Include(c => c.LoadingUnit)
               .Include(c => c.MaterialStatus)
               .Include(c => c.Item)
               .Include(c => c.CompartmentType)
               .Include(c => c.CompartmentStatus)
               .Include(c => c.PackageType)
               .Where(actualWhereFunc)
               .Select(c => new Compartment
               {
                   Id = c.Id,
                   CompartmentStatusDescription = c.CompartmentStatus.Description,
                   CompartmentTypeDescription = string.Format(
                       Resources.MasterData.CompartmentTypeListFormat,
                       c.CompartmentType.Width,
                       c.CompartmentType.Height),
                   ItemDescription = c.Item.Description,
                   IsItemPairingFixed = c.IsItemPairingFixed,
                   LoadingUnitCode = c.LoadingUnit.Code,
                   Lot = c.Lot,
                   MaterialStatusDescription = c.MaterialStatus.Description,
                   Stock = c.Stock,
                   Sub1 = c.Sub1,
                   Sub2 = c.Sub2
               }
               )
               .AsNoTracking();
        }

        #endregion Methods
    }
}
