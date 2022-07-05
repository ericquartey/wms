namespace Ferretto.VW.MAS.DataModels
{
    public class AlphaNumericBar : TcpIpAccessory
    {
        #region Properties

        /// <summary>
        /// Clear Alphanumeric bar message by closing pick or put view
        /// </summary>
        public bool? ClearAlphaBarOnCloseView { get; set; }

        public int MaxMessageLength { get; set; }

        public AlphaNumericBarSize Size { get; set; }

        #endregion
    }
}
