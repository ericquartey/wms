namespace Ferretto.WMS.Data.WebAPI.Contracts
{
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

                case var service when service == typeof(IMissionsDataService):
                    return new MissionsDataService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IAbcClassesDataService):
                    return new AbcClassesDataService(baseUrl.AbsoluteUri) as T;
            }

            return null;
        }

        #endregion
    }
}
