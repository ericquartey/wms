using System.Linq;

namespace Ferretto.Common.Modules.BLL.Services
{
    public interface IBusinessProvider
    {
        #region Methods

        IQueryable<Models.Compartment> GetAllCompartments();

        int GetAllCompartmentsCount();

        IQueryable<DataModels.CompartmentType> GetAllCompartmentTypes();

        IQueryable<DataModels.ItemCategory> GetAllItemCategories();

        IQueryable<DataModels.ItemManagementType> GetAllItemManagementTypes();

        IQueryable<Models.Item> GetAllItems();

        int GetAllItemsCount();

        IQueryable<Models.LoadingUnit> GetAllLoadingUnits();

        int GetAllLoadingUnitsCount();

        IQueryable<DataModels.MaterialStatus> GetAllMaterialStatuses();

        IQueryable<DataModels.MeasureUnit> GetAllMeasureUnits();

        IQueryable<DataModels.PackageType> GetAllPackageTypes();

        Models.CompartmentDetails GetCompartmentDetails(int compartmentId);

        IQueryable<Models.Compartment> GetCompartmentsByItemId(int itemId);

        Models.ItemDetails GetItemDetails(int itemId);

        IQueryable<Models.Item> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IQueryable<Models.Item> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        int Save(Models.ItemDetails itemDetails);

        int Save(Models.CompartmentDetails compartmentDetails);

        int Save(Models.LoadingUnitDetails loadingUnit);

        #endregion Methods
    }
}
