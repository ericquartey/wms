namespace Ferretto.Common.Controls.Interfaces
{
    public interface IRefreshDataEntityViewModel
    {
        #region Properties

        ColorRequired ColorRequired { get; set; }

        #endregion

        #region Methods

        void LoadRelatedData();

        #endregion
    }
}
