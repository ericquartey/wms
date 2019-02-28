namespace Ferretto.Common.Controls.Interfaces
{
    public interface IExtensionDataEntityViewModel
    {
        #region Properties

        ColorRequired ColorRequired { get; }

        #endregion

        #region Methods

        void LoadRelatedData();

        #endregion
    }
}
