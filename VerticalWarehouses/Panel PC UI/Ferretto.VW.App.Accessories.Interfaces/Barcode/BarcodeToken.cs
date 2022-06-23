namespace Ferretto.VW.App.Accessories.Interfaces
{
    /// <summary>
    /// the BarcodeToken must match the Group name of the regex Pattern defined in Barcode Actions
    /// </summary>
    public enum BarcodeToken
    {
        ListId,

        ItemCode,

        ItemQuantity,

        ItemBarcode,

        BearerToken,

        ItemSerialNumber,

        ItemLot,

        BasketCode,

        ShelfCode,

        IsDoubleConfirm,

        CarCode,

        MachineCode
    }
}
