namespace Ferretto.VW.MAS.DataModels
{
    public class CardReader : Accessory
    {
        #region Properties

        public bool? IsLocal { get; set; }

        public string TokenRegex { get; set; }

        #endregion
    }
}
