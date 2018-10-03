using System.Linq;
using Ferretto.Common.Modules.BLL.Models;

namespace Ferretto.Common.Modules.BLL.Services
{
    public interface IBusinessProvider
    {
        #region Methods

        IQueryable<Enumeration<int>> GetAllCellPositions();

        IQueryable<Compartment> GetAllCompartments();

        int GetAllCompartmentsCount();

        IQueryable<Enumeration<int>> GetAllCompartmentTypes();

        IQueryable<Enumeration<int>> GetAllItemCategories();

        IQueryable<Enumeration<int>> GetAllItemManagementTypes();

        IQueryable<Item> GetAllItems();

        int GetAllItemsCount();

        IQueryable<LoadingUnit> GetAllLoadingUnits();

        int GetAllLoadingUnitsCount();

        IQueryable<Enumeration<string>> GetAllLoadingUnitStatuses();

        IQueryable<Enumeration<int>> GetAllLoadingUnitTypes();

        IQueryable<Enumeration<int>> GetAllMaterialStatuses();

        IQueryable<Enumeration<string>> GetAllMeasureUnits();

        IQueryable<Enumeration<int>> GetAllPackageTypes();

        CompartmentDetails GetCompartmentDetails(int compartmentId);

        IQueryable<Compartment> GetCompartmentsByItemId(int itemId);

        ItemDetails GetItemDetails(int itemId);

        IQueryable<Models.Item> GetItemsWithAClass();

        int GetItemsWithAClassCount();

        IQueryable<Item> GetItemsWithFifo();

        int GetItemsWithFifoCount();

        LoadingUnitDetails GetLoadingUnitDetails(int loadingUnitId);

        int Save(ItemDetails itemDetails);

        int Save(CompartmentDetails compartmentDetails);

        int Save(LoadingUnitDetails loadingUnit);

        #endregion Methods
    }
}
