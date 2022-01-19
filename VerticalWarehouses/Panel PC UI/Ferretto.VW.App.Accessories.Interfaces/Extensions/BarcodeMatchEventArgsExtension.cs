namespace Ferretto.VW.App.Accessories.Interfaces
{
    public static class BarcodeMatchEventArgsExtensions
    {
        #region Methods

        public static string GetBearerToken(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.BearerToken.ToString(), out var bearerToken))
            {
                return bearerToken;
            }

            return null;
        }

        public static bool GetDoubleConfirm(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            return eventArgs.Parameters.ContainsKey(BarcodeToken.IsDoubleConfirm.ToString());
        }

        public static string GetItemBarCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ItemBarcode.ToString(), out var itemBarcode))
            {
                return itemBarcode;
            }

            return null;
        }

        public static string GetItemCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ItemCode.ToString(), out var code))
            {
                return code;
            }

            return null;
        }

        public static string GetItemLot(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ItemLot.ToString(), out var lot))
            {
                return lot;
            }

            return null;
        }

        public static double? GetItemQuantity(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ItemQuantity.ToString(), out var itemQuantityString))
            {
                if (double.TryParse(itemQuantityString, out var itemQuantity))
                {
                    return itemQuantity;
                }
            }

            return null;
        }

        public static string GetItemSerialNumber(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ItemSerialNumber.ToString(), out var serialNumber))
            {
                return serialNumber;
            }

            return null;
        }

        public static string GetListCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ListId.ToString(), out var listIdString))
            {
                return listIdString;
            }

            return null;
        }

        public static int? GetListId(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ListId.ToString(), out var listIdString)
                &&
                int.TryParse(listIdString, out var listId))
            {
                return listId;
            }

            return null;
        }

        public static void SetDoubleConfirm(this UserActionEventArgs eventArgs, bool isSet)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (isSet)
            {
                eventArgs.Parameters.Add(BarcodeToken.IsDoubleConfirm.ToString(), isSet.ToString());
            }
        }

        #endregion
    }
}
