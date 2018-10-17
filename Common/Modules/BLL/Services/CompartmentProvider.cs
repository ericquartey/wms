using System;
using System.Linq;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Modules.BLL.Services
{
    public class CompartmentProvider : ICompartmentProvider
    {
        #region Fields

        private readonly DatabaseContext dataContext;
        private readonly EnumerationProvider enumerationProvider;

        #endregion Fields

        #region Constructors

        public CompartmentProvider(DatabaseContext dataContext)
        {
            this.dataContext = dataContext;

            this.enumerationProvider = new EnumerationProvider(dataContext);
        }

        #endregion Constructors

        #region Methods

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
               .Select(c => new Compartment(c.Id)
               {
                   Code = c.Code,
                   CompartmentStatusDescription = c.CompartmentStatus.Description,
                   CompartmentTypeDescription = c.CompartmentType.Description,
                   ItemDescription = c.Item.Description,
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
            return this.dataContext.Compartments.Count();
        }

        public CompartmentDetails GetById(int id)
        {
            var compartmentDetails = this.dataContext.Compartments
               .Where(c => c.Id == id)
               .Include(c => c.LoadingUnit)
               .Include(c => c.Item)
               .Select(c => ProjectCompartmentDetails(c))
               .Single();

            compartmentDetails.CompartmentStatusChoices = this.enumerationProvider.GetAllCompartmentStatuses();
            compartmentDetails.CompartmentTypeChoices = this.enumerationProvider.GetAllCompartmentTypes();
            compartmentDetails.MaterialStatusChoices = this.enumerationProvider.GetAllMaterialStatuses();
            compartmentDetails.PackageTypeChoices = this.enumerationProvider.GetAllPackageTypes();

            return compartmentDetails;
        }

        public IQueryable<Compartment> GetByItemId(int id)
        {
            return this.dataContext.Compartments
                .Where(c => c.ItemId == id)
                .Include(c => c.LoadingUnit)
                .Include(c => c.CompartmentStatus)
                .Select(c => ProjectCompartment(c))
                .AsNoTracking();
        }

        public IQueryable<CompartmentDetails> GetByLoadingUnitId(int id)
        {
            return this.dataContext.Compartments
                .Where(c => c.LoadingUnitId == id)
                .Select(c => ProjectCompartmentDetails(c))
                .AsNoTracking();
        }

        public bool HasAnyAllowedItem(int modelId)
        {
            return this.dataContext.Compartments
                .Where(c => c.Id == modelId)
                .Include(c => c.CompartmentType)
                .ThenInclude(ct => ct.ItemsCompartmentTypes)
                .SelectMany(c => c.CompartmentType.ItemsCompartmentTypes)
                .AsNoTracking()
                .Any();
        }

        public int Save(CompartmentDetails model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var existingModel = this.dataContext.Compartments.Find(model.Id);

            this.dataContext.Entry(existingModel).CurrentValues.SetValues(model);

            return this.dataContext.SaveChanges();
        }

        private static Compartment ProjectCompartment(DataModels.Compartment c) =>
           new Compartment(c.Id)
           {
               Code = c.Code,
               CompartmentStatusDescription = c.CompartmentStatus?.Description,
               CompartmentTypeDescription = c.CompartmentType?.Description,
               ItemDescription = c.Item?.Description,
               LoadingUnitCode = c.LoadingUnit?.Code,
               Lot = c.Lot,
               MaterialStatusDescription = c.MaterialStatus?.Description,
               Stock = c.Stock,
               Sub1 = c.Sub1,
               Sub2 = c.Sub2,
               ItemPairing = c.ItemPairing.ToString(),
           };

        private static CompartmentDetails ProjectCompartmentDetails(DataModels.Compartment c) =>
            new CompartmentDetails(c.Id)
            {
                Code = c.Code,
                LoadingUnitCode = c.LoadingUnit?.Code,
                CompartmentTypeId = c.CompartmentTypeId,
                ItemPairing = c.ItemPairing.ToString(),
                ItemCode = c.Item?.Code,
                ItemDescription = c.Item?.Description,
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
                CreationDate = c.CreationDate,
                LastHandlingDate = c.LastHandlingDate,
                InventoryDate = c.InventoryDate,
                FirstStoreDate = c.FirstStoreDate,
                LastStoreDate = c.LastStoreDate,
                LastPickDate = c.LastPickDate,
                Width = c.Width,
                Height = c.Height,
                XPosition = c.XPosition,
                YPosition = c.YPosition
            };

        #endregion Methods
    }
}
