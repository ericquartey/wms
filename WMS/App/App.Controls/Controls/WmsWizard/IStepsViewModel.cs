namespace Ferretto.WMS.App.Controls
{
    public interface IStepsViewModel
    {
        #region Methods

        void Refresh();

        void UpdateError(string errorInfo);

        void SetIsSaveVisible(bool isSaveVisible);

        void UpdateCanSave();

        #endregion
    }
}
