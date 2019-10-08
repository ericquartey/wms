namespace Ferretto.VW.MAS.DataModels
{
    public class SetupProcedure : DataModel
    {
        #region Properties

        public double FeedRate { get; set; } = 1.0;

        public bool IsCompleted { get; set; }

        #endregion
    }
}
