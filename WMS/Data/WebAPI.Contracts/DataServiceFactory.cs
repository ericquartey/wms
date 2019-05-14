using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

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
        #region Fields

        private static readonly HttpClient Client = new HttpClient();

        #endregion

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
                case var service when service == typeof(ISchedulerHubClient):
                    return new SchedulerHubClient(baseUrl) as T;

                case var service when service == typeof(IAuthenticationService):
                    return new AuthenticationService(baseUrl, Client) as T;

                case var service when service == typeof(IItemsDataService):
                    return new ItemsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IItemListRowsDataService):
                    return new ItemListRowsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ILoadingUnitsDataService):
                    return new LoadingUnitsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IMachinesDataService):
                    return new MachinesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IMissionsDataService):
                    return new MissionsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ISchedulerRequestsDataService):
                    return new SchedulerRequestsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IAreasDataService):
                    return new AreasDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IAislesDataService):
                    return new AislesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IBaysDataService):
                    return new BaysDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICellsDataService):
                    return new CellsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICompartmentsDataService):
                    return new CompartmentsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IItemListsDataService):
                    return new ItemListsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IUsersDataService):
                    return new UsersDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IImagesDataService):
                    return new ImagesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IAbcClassesDataService):
                    return new AbcClassesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICellPositionsDataService):
                    return new CellPositionsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICellStatusesDataService):
                    return new CellStatusesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICellTypesDataService):
                    return new CellTypesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICompartmentStatusesDataService):
                    return new CompartmentStatusesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ICompartmentTypesDataService):
                    return new CompartmentTypesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IItemCategoriesDataService):
                    return new ItemCategoriesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ILoadingUnitStatusesDataService):
                    return new LoadingUnitStatusesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(ILoadingUnitTypesDataService):
                    return new LoadingUnitTypesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IMaterialStatusesDataService):
                    return new MaterialStatusesDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IMeasureUnitsDataService):
                    return new MeasureUnitsDataService(baseUrl.AbsoluteUri, Client) as T;

                case var service when service == typeof(IPackageTypesDataService):
                    return new PackageTypesDataService(baseUrl.AbsoluteUri, Client) as T;
            }

            return null;
        }

        #endregion
    }
}
