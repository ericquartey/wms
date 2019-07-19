namespace Ferretto.VW.App.Services
{
    public static class ServiceFactory
    {
        #region Methods

        public static T Get<T>()
            where T : class
        {
            switch (typeof(T))
            {
                case var service when service == typeof(ISessionService):
                    return new SessionService() as T;

                case var service when service == typeof(IThemeService):
                    return new ThemeService() as T;
            }

            return null;
        }

        #endregion
    }
}
