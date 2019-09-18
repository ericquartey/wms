namespace Ferretto.VW.App.Services
{
    public class ManualMovementshangedMessage
    {
        #region Constructors

        public ManualMovementshangedMessage(string viewModelName)
        {
            this.ViewModelName = viewModelName;
        }

        #endregion

        #region Properties

        public string ViewModelName { get; }

        #endregion
    }
}
