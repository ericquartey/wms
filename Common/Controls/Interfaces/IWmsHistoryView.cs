namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsHistoryView
    {
        #region Methods
        INavigableViewModel GetCurrentViewModel();

        void Appear(string moduleName, string viewModelName, object data);

        void Previous();

        #endregion
    }
}
