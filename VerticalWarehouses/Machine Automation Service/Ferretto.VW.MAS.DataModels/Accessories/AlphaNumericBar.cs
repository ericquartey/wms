namespace Ferretto.VW.MAS.DataModels
{
    public class AlphaNumericBar : TcpIpAccessory
    {
        #region Properties

        /// <summary>
        /// Clear Alphanumeric bar message by closing pick or put view
        /// </summary>
        public bool? ClearAlphaBarOnCloseView { get; set; }

        public string Field1 { get; set; }

        public string Field2 { get; set; }

        public string Field3 { get; set; }

        public string Field4 { get; set; }

        public string Field5 { get; set; }

        public int MaxMessageLength { get; set; }

        public AlphaNumericBarSize Size { get; set; }

        public bool? UseGet { get; set; }

        #endregion
    }
}
