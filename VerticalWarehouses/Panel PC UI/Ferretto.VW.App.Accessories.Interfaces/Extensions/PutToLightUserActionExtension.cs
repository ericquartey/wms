namespace Ferretto.VW.App.Accessories.Interfaces
{
    public static class PutToLightUserActionExtension
    {
        #region Methods

        public static string GetBasketCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.BasketCode.ToString(), out var itemBarcode))
            {
                return itemBarcode;
            }

            return null;
        }

        public static string GetCarCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.CarCode.ToString(), out var code))
            {
                return code;
            }

            return null;
        }

        public static string GetMachineCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.MachineCode.ToString(), out var code))
            {
                return code;
            }

            return null;
        }

        public static string GetShelfCode(this UserActionEventArgs eventArgs)
        {
            if (eventArgs is null)
            {
                throw new System.ArgumentNullException(nameof(eventArgs));
            }

            if (eventArgs.Parameters.TryGetValue(BarcodeToken.ShelfCode.ToString(), out var code))
            {
                return code;
            }

            return null;
        }

        #endregion
    }
}
