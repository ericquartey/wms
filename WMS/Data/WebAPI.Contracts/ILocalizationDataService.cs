namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public interface ILocalizationDataService
    {
        #region Methods

        void ClearAcceptedLanguage();

        void SetAcceptedLanguage(string languageCode);

        #endregion
    }
}
