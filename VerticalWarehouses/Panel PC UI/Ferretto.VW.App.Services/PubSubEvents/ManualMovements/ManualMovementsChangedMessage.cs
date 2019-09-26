namespace Ferretto.VW.App.Services
{
    public class ManualMovementsChangedMessage
    {
        #region Constructors

        public ManualMovementsChangedMessage(string viewModelName)
        {
            this.ViewModelName = viewModelName;
        }

        #endregion

        #region Properties

        public string ViewModelName { get; }

        #endregion
    }
}
