using System;
using System.Net;
using System.Net.Http;
using Ferretto.VW.App.Accessories.Barcode;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
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
            containerRegistry.RegisterSingleton<ILoadingUnitBarcodeService, LoadingUnitBarcodeService>();
            containerRegistry.RegisterSingleton<IPutToLightBarcodeService, PutToLightBarcodeService>();

            return containerRegistry;
        }

        #endregion
    }
}
