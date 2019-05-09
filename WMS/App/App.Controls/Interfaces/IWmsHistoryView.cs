namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IWmsHistoryView
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, object data);

        INavigableViewModel GetCurrentViewModel();

        void Previous();

        #endregion
    }
}
