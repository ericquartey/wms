using System;
using Prism.Ioc;

namespace Ferretto.VW.Devices.AlphaNumericBar
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureAlphaNumericBarDriver(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IAlphaNumericBarDriver, AlphaNumericBarDriver>();

            return containerRegistry;
        }

        #endregion
    }
}
