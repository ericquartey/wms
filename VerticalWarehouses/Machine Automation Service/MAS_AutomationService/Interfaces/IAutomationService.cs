namespace Ferretto.VW.MAS_AutomationService
{
    public interface IAutomationService
    {
        #region Properties

        int Number { get; set; }

        #endregion

        #region Methods

        void ExecuteHoming();

        #endregion
    }
}
