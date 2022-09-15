using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    internal sealed class MachineConfigurationService : IMachineConfigurationService
    {
        #region Fields

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings();

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        static MachineConfigurationService()
        {
            serializerSettings.Converters.Add(new CommonUtils.Converters.IPAddressConverter());
        }

        #endregion

        #region Properties

        public MAS.DataModels.VertimagConfiguration? Configuration { get; private set; }

        #endregion

        #region Methods

        public void ClearConfiguration()
        {
            this.Configuration = null;
        }

        public async Task LoadFromFileAsync(string fileName)
        {
            this.logger.Debug($"Loading vertimag configuration from file '{fileName}' ...");

            var fileContents = await File.ReadAllTextAsync(fileName);

            this.Deserialize(fileContents);

            this.logger.Debug($"Vertimag configuration loaded.");
        }

        public async Task LoadFromWebServiceAsync(Uri serviceUri)
        {
            this.logger.Debug($"Loading vertimag configuration from web service '{serviceUri}' ...");

            if (serviceUri is null)
            {
                throw new ArgumentNullException(nameof(serviceUri));
            }

            using var httpClient = new System.Net.Http.HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(2)
            };

            var httpResponseMessage = await httpClient.GetAsync(serviceUri);

            var responseContents = await httpResponseMessage.Content.ReadAsStringAsync();

            this.Deserialize(responseContents);

            this.logger.Debug($"Vertimag configuration loaded.");
        }

        public async Task SaveToFileAsync(string configurationFilePath)
        {
            this.logger.Debug($"Saving vertimag configuration to file '{configurationFilePath}' ...");

            try
            {
                var fileContents = JsonConvert.SerializeObject(this.Configuration, serializerSettings);

                this.logger.Debug($"Serialize ok.");

                await File.WriteAllTextAsync(configurationFilePath, fileContents);

                this.logger.Debug($"Vertimag configuration saved successfully.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error while saving file.");

                var msg = $"Error writing configuration file '{configurationFilePath}'.";
                throw new InvalidOperationException(msg, ex);
            }
        }

        private void Deserialize(string configuration)
        {
            this.Configuration = JsonConvert.DeserializeObject<MAS.DataModels.VertimagConfiguration>(
                configuration,
                serializerSettings);

            if (this.Configuration is null)
            {
                throw new Exception("Could not load configuration.");
            }
        }

        #endregion
    }
}
