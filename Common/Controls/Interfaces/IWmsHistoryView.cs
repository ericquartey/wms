namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsHistoryView
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, object data);

        void Previous();

        #endregion Methods
    }
}
