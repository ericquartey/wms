namespace Ferretto.VW.App.Accessories
{
    public static class BarcodeMatchEventArgsExtensions
    {
        #region Methods

        public static string GetItemBarCode(this BarcodeMatchEventArgs eventArgs)
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

        public static int? GetListId(this BarcodeMatchEventArgs eventArgs)
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
