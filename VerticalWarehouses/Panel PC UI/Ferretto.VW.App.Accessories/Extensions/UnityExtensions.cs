using System;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.Devices.BarcodeReader.Newland;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.Devices.TokenReader;
using Ferretto.VW.Devices.WeightingScale;
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

            containerRegistry.ConfigureAlphaNumericBarDriver();
            //containerRegistry.RegisterSingleton<IAlphaNumericBarService, AlphaNumericBarService>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureBarcodeReaderUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.ConfigureNewlandBarcodeReaderDriver();
            containerRegistry.RegisterSingleton<IBarcodeReaderService, BarcodeReaderService>();

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

            containerRegistry.ConfigureLaserPointerDriver();
            //containerRegistry.RegisterSingleton<ILaserPointerService, LaserPointerService>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureTokenReaderUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.ConfigureTokenReaderDriver();
            containerRegistry.RegisterSingleton<ITokenReaderService, TokenReaderService>();

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureWeightingScaleUiServices(this IContainerRegistry containerRegistry)
        {
            if (containerRegistry is null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            containerRegistry.ConfigureWeightingScaleDriver();
            containerRegistry.RegisterSingleton<IWeightingScaleService, WeightingScaleService>();

            return containerRegistry;
        }

        #endregion
    }
}
