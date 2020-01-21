using Ferretto.VW.Simulator.Services.Interfaces;

namespace Ferretto.VW.Simulator.Services
{
    public static class ServiceFactory
    {
        #region Methods

        public static T Get<T>()
            where T : class
        {
            switch (typeof(T))
            {
                case var service when service == typeof(IThemeService):
                    return new ThemeService() as T;

                case var service when service == typeof(IMachineService):
                    return new MachineService() as T;
            }

            return null;
        }

        #endregion
    }
}
