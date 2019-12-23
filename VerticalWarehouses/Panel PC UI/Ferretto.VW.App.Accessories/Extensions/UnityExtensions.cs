using System;
using Ferretto.VW.Devices.BarcodeReader;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.App.Accessories
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry UseBarcodeReader(
            this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IBarcodeReaderService, BarcodeReaderService>();

            return containerRegistry;
        }

        #endregion
    }
}
