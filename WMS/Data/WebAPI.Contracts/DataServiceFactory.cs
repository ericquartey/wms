using System.Diagnostics.CodeAnalysis;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "Ok")]
    [SuppressMessage(
        "Microsoft.Maintainability",
        "CA1502",
        Justification = "OK")]
    public static class DataServiceFactory
    {
        #region Methods

        public static T GetService<T>(System.Uri baseUrl)
            where T : class
        {
            if (baseUrl == null)
            {
                throw new System.ArgumentNullException(nameof(baseUrl));
            }

            switch (typeof(T))
            {
                case var service when service == typeof(IItemsDataService):
                    return new ItemsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemListRowsDataService):
                    return new ItemListRowsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ILoadingUnitsDataService):
                    return new LoadingUnitsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IMachinesDataService):
                    return new MachinesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IMissionsDataService):
                    return new MissionsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ISchedulerRequestsDataService):
                    return new SchedulerRequestsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IAreasDataService):
                    return new AreasDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IAislesDataService):
                    return new AislesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IBaysDataService):
                    return new BaysDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICellsDataService):
                    return new CellsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICompartmentsDataService):
                    return new CompartmentsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IUsersDataService):
                    return new UsersDataService(baseUrl.AbsoluteUri) as T;

                // ENUMERATION
                case var service when service == typeof(IItemCompartmentTypesDataService):
                    return new ItemCompartmentTypesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IAbcClassesDataService):
                    return new AbcClassesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICellPositionsDataService):
                    return new CellPositionsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICellStatusesDataService):
                    return new CellStatusesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICellTypesDataService):
                    return new CellTypesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICompartmentStatusesDataService):
                    return new CompartmentStatusesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ICompartmentTypesDataService):
                    return new CompartmentTypesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemCategoriesDataService):
                    return new ItemCategoriesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ILoadingUnitStatusesDataService):
                    return new LoadingUnitStatusesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(ILoadingUnitTypesDataService):
                    return new LoadingUnitTypesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IMaterialStatusesDataService):
                    return new MaterialStatusesDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IMeasureUnitsDataService):
                    return new MeasureUnitsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IPackageTypesDataService):
                    return new PackageTypesDataService(baseUrl.AbsoluteUri) as T;
            }

            return null;
        }

        #endregion
    }
}
