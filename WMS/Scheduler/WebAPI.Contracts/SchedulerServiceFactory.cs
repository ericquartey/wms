namespace Ferretto.WMS.Scheduler.WebAPI.Contracts
{
    public static class SchedulerServiceFactory
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
                case var service when service == typeof(IItemsSchedulerService):
                    return new ItemsSchedulerService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemListsSchedulerService):
                    return new ItemListsSchedulerService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IItemListRowsSchedulerService):
                    return new ItemListRowsSchedulerService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IMissionsSchedulerService):
                    return new MissionsSchedulerService(baseUrl.AbsoluteUri) as T;

                case var service when service == typeof(IBaysSchedulerService):
                    return new BaysSchedulerService(baseUrl.AbsoluteUri) as T;
            }

            return null;
        }

        #endregion Methods
    }
}
