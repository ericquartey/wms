using System;
using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CompartmentProvider(EnumerationProvider enumerationProvider)
        {
            this.enumerationProvider = enumerationProvider;
        }

        #endregion Constructors

        #region Methods

        public Int32 Add(CompartmentDetails model)
        {
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model));
                }

                context.Compartments.Add(new DataModels.Compartment
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

                return context.SaveChanges();
            }
        }

        public void Delete(Int32 id)
        {
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                var existingModel = context.Compartments.Find(id);
                if (existingModel != null)
                {
                    context.Remove(existingModel);
                }
                context.SaveChanges();
            }
        }

        public IQueryable<Compartment> GetAll()
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return context.Compartments
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
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                return context.Compartments.Count();
            }
        }

        public CompartmentDetails GetById(int id)
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            var compartmentDetails = context.Compartments
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
                   ItemId = c.ItemId
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

        public IQueryable<Compartment> GetByItemId(int id)
        {
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return context.Compartments
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
            var context = ServiceLocator.Current.GetInstance<DatabaseContext>();

            return context.Compartments
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
                    ItemId = c.ItemId
                })
                .AsNoTracking();
        }

        public bool HasAnyAllowedItem(int modelId)
        {
            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                return context.Compartments
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

            using (var context = ServiceLocator.Current.GetInstance<DatabaseContext>())
            {
                var existingModel = context.Compartments.Find(model.Id);

                context.Entry(existingModel).CurrentValues.SetValues(model);

                return context.SaveChanges();
            }
        }

        #endregion Methods
    }
}
