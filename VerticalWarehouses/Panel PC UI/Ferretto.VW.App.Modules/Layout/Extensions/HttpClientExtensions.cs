using System.Net.Http;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Layout.Extensions
{
    public static class HttpClientExtensions
    {
        #region Methods

        public async static Task<string> GetContentFromServiceAsync(this HttpClient httpClient, string serviceUri)
        {
            var httpResponseMessage = await httpClient.GetAsync(serviceUri);
            return await httpResponseMessage.Content.ReadAsStringAsync();
        }

        #endregion
    }
}
