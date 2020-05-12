namespace Ferretto.VW.MAS.DataModels
{
    public sealed class BayProfileCheckProcedure : SetupProcedure
    {
        #region Properties

        public double ProfileCorrectDistance { get; set; } = 150D;

        public double ProfileDegrees { get; set; } = 5D;

        public double ProfileTotalDistance { get; set; } = 265D;

        #endregion
    }
}
