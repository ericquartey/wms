namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsHistoryView
    {
        #region Methods

        void Appear(string moduleName, string viewModelName);

        void Previous();

        #endregion Methods
    }
}
