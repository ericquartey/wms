using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    [SuppressMessage(
        "Microsoft.Maintainability",
        "CA1502",
        Justification = "OK")]
    public static class DataServiceFactory
    {
        #region Methods

        public static T GetService<T>(System.Uri baseUrl, HttpClient httpClient = null)
            where T : class
        {
            if (baseUrl == null)
            {
                throw new System.ArgumentNullException(nameof(baseUrl));
            }

            var client = httpClient ?? new HttpClient { BaseAddress = baseUrl };

            switch (typeof(T))
            {
                case var service when service == typeof(ILocalizationDataService):
                    return new LocalizationDataService(client) as T;

                case var service when service == typeof(IDataHubClient):
                    return new DataHubClient(baseUrl) as T;

                case var service when service == typeof(IItemsDataService):
                    return new ItemsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IItemListRowsDataService):
                    return new ItemListRowsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ILoadingUnitsDataService):
                    return new LoadingUnitsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IMachinesDataService):
                    return new MachinesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IMissionsDataService):
                    return new MissionsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IMissionOperationsDataService):
                    return new MissionOperationsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ISchedulerRequestsDataService):
                    return new SchedulerRequestsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IAreasDataService):
                    return new AreasDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IAislesDataService):
                    return new AislesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IBaysDataService):
                    return new BaysDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICellsDataService):
                    return new CellsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICompartmentsDataService):
                    return new CompartmentsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IUsersDataService):
                    return new UsersDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IImagesDataService):
                    return new ImagesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IAbcClassesDataService):
                    return new AbcClassesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICellPositionsDataService):
                    return new CellPositionsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICellStatusesDataService):
                    return new CellStatusesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICellTypesDataService):
                    return new CellTypesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICompartmentStatusesDataService):
                    return new CompartmentStatusesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ICompartmentTypesDataService):
                    return new CompartmentTypesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IItemCategoriesDataService):
                    return new ItemCategoriesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ILoadingUnitStatusesDataService):
                    return new LoadingUnitStatusesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(ILoadingUnitTypesDataService):
                    return new LoadingUnitTypesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IMaterialStatusesDataService):
                    return new MaterialStatusesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IMeasureUnitsDataService):
                    return new MeasureUnitsDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IPackageTypesDataService):
                    return new PackageTypesDataService(baseUrl.AbsoluteUri, client) as T;

                case var service when service == typeof(IGlobalSettingsDataService):
                    return new GlobalSettingsDataService(baseUrl.AbsoluteUri, client) as T;
            }

            return null;
        }

        #endregion
    }
}
