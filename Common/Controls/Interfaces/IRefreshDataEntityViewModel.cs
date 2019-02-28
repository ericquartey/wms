namespace Ferretto.Common.Controls.Interfaces
{
    public interface IRefreshDataEntityViewModel
    {
        #region Properties

        ColorRequired ColorRequired { get; }

        #endregion

        #region Methods

        void LoadRelatedData();

        #endregion
    }
}
