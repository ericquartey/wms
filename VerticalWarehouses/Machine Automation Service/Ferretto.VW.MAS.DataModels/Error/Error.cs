namespace Ferretto.VW.MAS.DataModels
{
    public class Error
    {
        #region Properties

        public int Code { get; set; }

        public string Description { get; set; }

        public int Issue { get; set; }

        public string Reason { get; set; }

        public ErrorStatistic Statistics { get; set; }

        #endregion
    }
}
