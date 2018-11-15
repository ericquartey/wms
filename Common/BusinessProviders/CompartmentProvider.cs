using System;
using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.Common.BusinessProviders
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CompartmentProvider(
            DatabaseContext context,
            EnumerationProvider enumerationProvider)
        {
            this.dataContext = context;
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public int Add(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            lock (this.dataContext)
            {
                this.dataContext.Compartments.Add(new DataModels.Compartment
                {
                    Width = model.Width,
                    Height = model.Height,
                    XPosition = model.XPosition,
                    YPosition = model.YPosition,
                    LoadingUnitId = model.LoadingUnitId,
                    CompartmentTypeId = model.CompartmentTypeId,
                    ItemPairing = DataModels.Pairing.Free,
                    Stock = model.Stock,
                    ReservedForPick = model.ReservedForPick,
                    ReservedToStore = model.ReservedToStore,
                    CreationDate = DateTime.Now
                });

                return this.dataContext.SaveChanges();
            }
        }

        public int Delete(int id)
        {
            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Compartments.Find(id);
                if (existingModel != null)
                {
                    this.dataContext.Remove(existingModel);
                }
                return this.dataContext.SaveChanges();
            }
        }

        public IQueryable<Compartment> GetAll()
        {
            return this.dataContext.Compartments
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
                   ItemPairingDescription = c.ItemPairing.ToString(),
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
            lock (this.dataContext)
            {
                return this.dataContext.Compartments.Count();
            }
        }

        public CompartmentDetails GetById(int id)
        {
            lock (this.dataContext)
            {
                var compartmentDetails = this.dataContext.Compartments
                   .Where(c => c.Id == id)
                   .Include(c => c.LoadingUnit)
                   .Include(c => c.Item)
                   .Include(c => c.CompartmentStatus)
                   .Select(c => new CompartmentDetails
                   {
                       Id = c.Id,
                       Code = c.Code,
                       LoadingUnitCode = c.LoadingUnit.Code,
                       CompartmentTypeId = c.CompartmentTypeId,
                       ItemPairing = (int)c.ItemPairing,
                       ItemCode = c.Item.Code,
                       ItemDescription = c.Item.Description,
                       Sub1 = c.Sub1,
                       Sub2 = c.Sub2,
                       MaterialStatusId = c.MaterialStatusId,
                       FifoTime = c.FifoTime,
                       PackageTypeId = c.PackageTypeId,
                       Lot = c.Lot,
                       RegistrationNumber = c.RegistrationNumber,
                       MaxCapacity = c.MaxCapacity,
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
                       ItemPairingDescription = c.ItemPairing.ToString()
                   })
                   .Single();

                compartmentDetails.CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses();
                compartmentDetails.CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes();
                compartmentDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
                compartmentDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();
                compartmentDetails.ItemPairingChoices =
                    ((DataModels.Pairing[])Enum.GetValues(typeof(DataModels.Pairing)))
                    .Select(i => new Enumeration((int)i, i.ToString())).ToList();

                return compartmentDetails;
            }
        }

        public IQueryable<Compartment> GetByItemId(int id)
        {
            return this.dataContext.Compartments
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
                    ItemPairingDescription = c.ItemPairing.ToString(),
                })
                .AsNoTracking();
        }

        public IQueryable<CompartmentDetails> GetByLoadingUnitId(int id)
        {
            return this.dataContext.Compartments
                .Where(c => c.LoadingUnitId == id)
                .Include(c => c.LoadingUnit)
                .Include(c => c.Item)
                .Include(c => c.CompartmentStatus)
                .Select(c => new CompartmentDetails
                {
                    Id = c.Id,
                    Code = c.Code,
                    LoadingUnitCode = c.LoadingUnit.Code,
                    CompartmentTypeId = c.CompartmentTypeId,
                    ItemPairing = (int)c.ItemPairing,
                    ItemCode = c.Item.Code,
                    ItemDescription = c.Item.Description,
                    Sub1 = c.Sub1,
                    Sub2 = c.Sub2,
                    MaterialStatusId = c.MaterialStatusId,
                    FifoTime = c.FifoTime,
                    PackageTypeId = c.PackageTypeId,
                    Lot = c.Lot,
                    RegistrationNumber = c.RegistrationNumber,
                    MaxCapacity = c.MaxCapacity,
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
                    ItemPairingDescription = c.ItemPairing.ToString(),
                })
                .AsNoTracking();
        }

        public bool HasAnyAllowedItem(int modelId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Compartments
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

            lock (this.dataContext)
            {
                var existingModel = this.dataContext.Compartments.Find(model.Id);

                this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

                return this.dataContext.SaveChanges();
            }
        }

        #endregion Methods
    }
}
