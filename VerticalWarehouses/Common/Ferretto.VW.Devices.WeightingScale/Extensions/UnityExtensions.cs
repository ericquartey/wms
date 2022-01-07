using System;
using Prism.Ioc;
using Unity;

namespace Ferretto.VW.Devices.WeightingScale
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureWeightingScaleDriver(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IWeightingScaleDriverDini, WeightingScaleDriverDini>();

            containerRegistry.RegisterSingleton<IWeightingScaleDriverMinebea, WeightingScaleDriverMinebea>();

            return containerRegistry;
        }

        #endregion
    }
}
