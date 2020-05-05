using System;
using System.Collections;
using System.Collections.Generic;
using Prism.Ioc;
using Unity;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureMockBarcodeReader(
            this IContainerRegistry containerRegistry,
            IList<string> barcodes,
            int intervalMilliseconds)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterInstance<IBarcodeReaderDriver>(
#pragma warning disable CA2000 // Dispose objects before losing scope
                // Justification: here we are registering a singleton instance in the container
                new MockReader(barcodes, intervalMilliseconds));
#pragma warning restore CA2000 // Dispose objects before losing scope

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureNewlandBarcodeReader(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IBarcodeReaderDriver, BarcodeReader>();

            return containerRegistry;
        }

        #endregion
    }
}
