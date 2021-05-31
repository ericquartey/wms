namespace Ferretto.VW.MAS.DataModels
{
    public class AlphaNumericBar : TcpIpAccessory
    {
        #region Properties

        public int MaxMessageLength { get; set; } = 125;

        public AlphaNumericBarSize Size { get; set; }

        #endregion
    }
}
