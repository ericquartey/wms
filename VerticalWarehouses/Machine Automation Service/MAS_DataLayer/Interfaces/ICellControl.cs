namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ICellControl
    {
        #region Properties

        decimal FeedRateCC { get; }

        decimal StepValueCC { get; }

        #endregion
    }
}
