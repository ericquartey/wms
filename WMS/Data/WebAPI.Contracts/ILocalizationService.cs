namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public interface ILocalizationService
    {
        #region Methods

        void ClearAcceptedLanguage();

        void SetAcceptedLanguage(string languageCode);

        #endregion
    }
}
