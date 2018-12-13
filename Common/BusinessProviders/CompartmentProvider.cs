using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly CompartmentTypeProvider compartmentTypeProvider;
        private readonly IDatabaseContextService dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CompartmentProvider(
            IDatabaseContextService context,
            EnumerationProvider enumerationProvider,
            CompartmentTypeProvider compartmentTypeProvider)
        {
            this.dataContext = context;
            this.enumerationProvider = enumerationProvider;
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion Constructors

        #region Methods

        public async Task<OperationResult> Add(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            //TODO: use Transaction

            try
            {
                var dataContext = this.dataContext.Current;

                var typeId = await this.compartmentTypeProvider.Add(new CompartmentType
                {
                    Width = model.Width,
                    Height = model.Height,
                    Description = ""
                }, model.ItemId, model.MaxCapacity);

                var entry = dataContext.Compartments.Add(new DataModels.Compartment
                {
                    Width = model.Width,
                    Height = model.Height,
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    CompartmentTypeId = typeId.EntityId.Value,
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

                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                return new OperationResult(false, description: ex.Message);
            }
        }

        public int Delete(int id)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.Compartments.Find(id);
                if (existingModel != null)
                {
                    dataContext.Remove(existingModel);
                }
                return dataContext.SaveChanges();
            }
        }

        public IQueryable<Compartment> GetAll()
        {
            return this.dataContext.Current.Compartments
               .Include(c => c.LoadingUnit)
               .Include(c => c.MaterialStatus)
               .Include(c => c.Item)
               .Include(c => c.CompartmentType)
               .Include(c => c.CompartmentStatus)
               .Include(c => c.PackageType)
               .Select(c => new Compartment
               {
                   Id = c.Id,
                   Code = c.Code,
                   CompartmentStatusDescription = c.CompartmentStatus.Description,
                   CompartmentTypeDescription = c.CompartmentType.Description,
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
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Compartments.Count();
            }
        }

        public CompartmentDetails GetById(int id)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var compartmentList = dataContext.Compartments
                   .Where(c => c.Id == id)
                   .Include(c => c.LoadingUnit)
                   .Include(c => c.Item)
                   .Include(c => c.CompartmentStatus)
                   .GroupJoin(
                        dataContext.ItemsCompartmentTypes,
                        cmp => new { CompartmentTypeId = cmp.CompartmentTypeId, ItemId = cmp.ItemId.Value },
                        ict => new { CompartmentTypeId = ict.CompartmentTypeId, ItemId = ict.ItemId },
                        (cmp, ict) => new { cmp, ict = ict.DefaultIfEmpty() }
                    )
                   .Select(j => new CompartmentDetails
                   {
                       Id = j.cmp.Id,
                       Code = j.cmp.Code,
                       LoadingUnitCode = j.cmp.LoadingUnit.Code,
                       CompartmentTypeId = j.cmp.CompartmentTypeId,
                       IsItemPairingFixed = c.IsItemPairingFixed,
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
                       Width = j.cmp.Width,
                       Height = j.cmp.Height,
                       XPosition = j.cmp.XPosition,
                       YPosition = j.cmp.YPosition,
                       LoadingUnitId = j.cmp.LoadingUnitId,
                       ItemId = j.cmp.ItemId,
                   })
                   .ToList();

                var compartmentDetails = compartmentList.Single();

                compartmentDetails.CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses();
                compartmentDetails.CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes();
                compartmentDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
                compartmentDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();

                return compartmentDetails;
            }
        }

        public IQueryable<Compartment> GetByItemId(int id)
        {
            return this.dataContext.Current.Compartments
                .Where(c => c.ItemId == id)
                .Include(c => c.LoadingUnit)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.CompartmentType)
                .Include(c => c.Item)
                .Include(c => c.MaterialStatus)
                .Select(c => new Compartment
                {
                    Id = c.Id,
                    Code = c.Code,
                    CompartmentStatusDescription = c.CompartmentStatus.Description,
                    CompartmentTypeDescription = c.CompartmentType.Description,
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
            return this.dataContext.Current.Compartments
                .Where(c => c.LoadingUnitId == id)
                .Include(c => c.LoadingUnit)
                .Include(c => c.Item)
                .Include(c => c.CompartmentStatus)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .Select(c => new CompartmentDetails
                {
                    Id = c.Id,
                    Code = c.Code,
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
                    Width = c.Width,
                    Height = c.Height,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                    LoadingUnitId = c.LoadingUnitId,
                    ItemId = c.ItemId,
                    IsItemPairingFixed = c.IsItemPairingFixed,
                })
                .AsNoTracking();
        }

        public CompartmentDetails GetNewCompartmentDetails()
        {
            var compartmentDetails = new CompartmentDetails();

            compartmentDetails.CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses();
            compartmentDetails.CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes();
            compartmentDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
            compartmentDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();

            compartmentDetails.MaterialStatusId = compartmentDetails.MaterialStatusChoices.FirstOrDefault()?.Id;

            return compartmentDetails;
        }

        public bool HasAnyAllowedItem(int modelId)
        {
            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                return dataContext.Compartments
                    .Where(c => c.Id == modelId)
                    .Include(c => c.CompartmentType)
                    .ThenInclude(ct => ct.ItemsCompartmentTypes)
                    .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                    .AsNoTracking()
                    .Any();
            }
        }

        public int Save(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var dataContext = this.dataContext.Current;
            lock (dataContext)
            {
                var existingModel = dataContext.Compartments.Find(model.Id);

                dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
