using System.Net.Http;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    internal class LocalizationDataService : ILocalizationDataService
    {
        #region Fields

        private const string AcceptLanguage = "accept-language";

        private readonly HttpClient client;

        #endregion

        #region Constructors

        public LocalizationDataService(HttpClient client)
        {
            this.client = client;
        }

        #endregion

        #region Methods

        public void ClearAcceptedLanguage()
        {
            this.client.DefaultRequestHeaders.Remove(AcceptLanguage);
        }

        public void SetAcceptedLanguage(string languageCode)
        {
            this.client.DefaultRequestHeaders.Add(AcceptLanguage, languageCode);
        }

        #endregion
    }
}
