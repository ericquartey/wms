using Ferretto.Common.EF;

namespace Ferretto.WMS.Data.Core
{
    public static class ProviderFactory
    {
        #region Methods

        public static T Get<T>(DatabaseContext databaseContext)
            where T : class
        {
            var interfaceType = typeof(T);
            var instanceTypeName = interfaceType.Name.Substring(1);

            return typeof(ProviderFactory).Assembly.GetType(instanceTypeName)?
                .GetConstructor(new[] { typeof(DatabaseContext) })?
                .Invoke(new object[] { databaseContext }) as T;
        }

        #endregion
    }
}
