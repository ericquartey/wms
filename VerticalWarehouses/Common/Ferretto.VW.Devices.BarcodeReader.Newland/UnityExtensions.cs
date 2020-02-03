using System;
using Ferretto.VW.Devices.BarcodeReader;
using Prism.Events;
using Prism.Ioc;
using Prism.Unity;
using Unity;
using Unity.Injection;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureMockBarcodeReader(
            this IContainerRegistry containerRegistry,
            ConfigurationOptions options)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterInstance<IBarcodeConfigurationOptions>(options);
            containerRegistry.RegisterSingleton<IBarcodeReader, MockReader>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureNewlandBarcodeReader(
                   this IContainerRegistry containerRegistry,
           ConfigurationOptions options)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterInstance<IBarcodeConfigurationOptions>(options);
            containerRegistry.RegisterSingleton<IBarcodeReader, BarcodeReader>();

            return containerRegistry;
        }

        #endregion
    }
}
