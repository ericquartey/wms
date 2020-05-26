using System;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.Devices.AlphaNumericBar;
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

            containerRegistry.RegisterInstance<IAlphaNumericBarDriver>(new AlphaNumericBarDriver());  //TODO: da verificare, non è containerRegistry.RegisterSingleton<IAlphaNumericBarDriver>(driver)

            return containerRegistry;
        }

        public static IContainerRegistry ConfigureBarcodeReaderUiServices(this IContainerRegistry containerRegistry)
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
