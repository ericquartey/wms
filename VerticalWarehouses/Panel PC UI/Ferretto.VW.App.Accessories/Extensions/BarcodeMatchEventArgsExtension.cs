namespace Ferretto.VW.App.Accessories
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

            if (eventArgs.Parameters.TryGetValue(BarcodeTokens.BearerToken.ToString(), out var bearerToken))
            {
                return bearerToken;
            }

            return null;
        }

        public static string GetItemBarCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeTokens.ItemBarcode.ToString(), out var itemBarcode))
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

            if (eventArgs.Parameters.TryGetValue(BarcodeTokens.ItemCode.ToString(), out var itemBarcode))
            {
                return itemBarcode;
            }

            return null;
        }

        public static double? GetItemQuantity(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeTokens.ItemQuantity.ToString(), out var itemQuantityString))
            {
                if (double.TryParse(itemQuantityString, out var itemQuantity))
                {
                    return itemQuantity;
                }
            }

            return null;
        }

        public static int? GetListId(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeTokens.ListId.ToString(), out var listIdString)
                &&
                int.TryParse(listIdString, out var listId))
            {
                return listId;
            }

            return null;
        }

        #endregion
    }
}
