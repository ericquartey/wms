namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ErrorStatistic
    {
        #region Properties

        public int Code { get; set; }

        public ErrorDefinition Error { get; set; }

        public int TotalErrors { get; set; }

        #endregion
    }
}
