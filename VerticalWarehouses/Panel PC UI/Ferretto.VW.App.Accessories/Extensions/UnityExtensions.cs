using System;
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

            containerRegistry.ConfigureAlphaNumericBarDriver();

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

            /*
             * For barcode reader debugging purposes

            var barcodes = new[]
            {
                // user tokens
                // "1234",
                // "5678",

                // item actions
                // "ITEM_B8450BDD_FILTER",
                // "ITEM_ABC_FILTER",
                // "ITEM_B8450BDD_QTY5_PICK",

                // list actions
                // "LIST_100_EXEC",
                // "LIST_01_EXEC",
                // "LIST_0001_FILTER",

                // operations

                // "B8450BDD",
                // "FE8AFA5A",
                // "B8450BDD_LOT_ABC_SN_123",

                // "#CONFIRM#",
                // "C#",
                // "#RECALL_LU",

                // "SN#123",
                // "LOT#ABC",

                // put to light
                "#PTL_OPEN",
                "#BASKET456",
                "#SHELF123",

                "#PTL_CLOSE",
                "#BASKET456",
                "#SHELF123",
            };
            containerRegistry.ConfigureMockBarcodeReader(barcodes, 5000);
             */

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

        #endregion
    }
}
