using System;
using Prism.Ioc;
using Unity;

namespace Ferretto.VW.Devices.LaserPointer
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureLaserPointerDriver(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<ILaserPointerDriver, LaserPointerDriver>();

            return containerRegistry;
        }

        #endregion
    }
}
