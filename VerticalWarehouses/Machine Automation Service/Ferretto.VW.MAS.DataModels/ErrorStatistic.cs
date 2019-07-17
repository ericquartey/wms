namespace Ferretto.VW.MAS.DataModels
{
    public class ErrorStatistic
    {
        #region Properties

        public int Code { get; set; }

        public Error Error { get; set; }

        public int TotalErrors { get; set; }

        #endregion
    }
}
