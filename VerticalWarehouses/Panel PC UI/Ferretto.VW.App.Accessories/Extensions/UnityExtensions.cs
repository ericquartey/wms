using System;
using Ferretto.VW.App.Accessories.AlphaNumericBar;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.Devices.BarcodeReader.Newland;
using Prism.Ioc;
using Unity;

namespace Ferretto.VW.App.Accessories
{
    public static class UnityExtensions
    {
        #region Methods

        public static IContainerRegistry ConfigureAlphaNumericBarUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IAlphaNumericBarService, AlphaNumericBarService>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureBarcodeReaderUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<IBarcodeReaderService, BarcodeReaderService>();
            containerRegistry.ConfigureNewlandBarcodeReaderDriver();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureCardReaderUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<ICardReaderService, KeyboarEmulatedCardReaderService>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureLaserPointerUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.RegisterSingleton<ILaserPointerService, LaserPointerService>();

            return containerRegistry;
        }

        #endregion
    }
}
