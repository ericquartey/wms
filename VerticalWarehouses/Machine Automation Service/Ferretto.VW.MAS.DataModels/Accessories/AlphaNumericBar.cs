namespace Ferretto.VW.MAS.DataModels
{
    public class AlphaNumericBar : TcpIpAccessory
    {
        #region Properties

        public int MaxMessageLength { get; set; }

        public AlphaNumericBarSize Size { get; set; }

        #endregion
    }
}
