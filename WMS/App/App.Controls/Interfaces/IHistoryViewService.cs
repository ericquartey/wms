namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IHistoryViewService
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, object data = null);

        void Previous();

        #endregion
    }
}
