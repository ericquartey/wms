namespace Ferretto.VW.MAS.DataModels
{
    public sealed class RepeatedTestProcedure : SetupProcedure
    {
        #region Properties

        public int PerformedCycles { get; set; }

        public int RequiredCycles { get; set; }

        #endregion
    }
}
