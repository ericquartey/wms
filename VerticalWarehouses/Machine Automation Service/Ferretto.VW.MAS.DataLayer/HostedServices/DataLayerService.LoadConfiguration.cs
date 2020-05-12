using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;

namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

        private static void ValidateJson(JObject jsonObject)
        {
            try
            {
                using (var streamReader = new StreamReader("configuration/schemas/vertimag-configuration-schema.json"))
                {
                    using (var textReader = new JsonTextReader(streamReader))
                    {
                        var schema = JSchema.Load(textReader);
                        jsonObject.Validate(schema);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        private async Task LoadConfigurationAsync(string configurationFilePath, DataLayerContext dataContext)
        {
            if (dataContext.Machines.Any())
            {
                return;
            }

            this.Logger.LogInformation($"First run: loading configuration from JSON file ...");

            string fileContents;
            using (var streamReader = new StreamReader(configurationFilePath))
            {
                fileContents = await streamReader.ReadToEndAsync();
            }

            var jsonObject = JObject.Parse(fileContents);

            ValidateJson(jsonObject);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());

            var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);
            vertimagConfiguration.Validate();

            dataContext.Machines.Add(vertimagConfiguration.Machine);
            dataContext.LoadingUnits.AddRange(vertimagConfiguration.LoadingUnits);
            dataContext.SetupProceduresSets.Add(vertimagConfiguration.SetupProcedures);
            if (vertimagConfiguration.Wms != null)
            {
                var wmsSettings = dataContext.WmsSettings.Single();
                vertimagConfiguration.Wms.Id = wmsSettings.Id;
                dataContext.AddOrUpdate(vertimagConfiguration.Wms, s => s.Id);
            }

            if (vertimagConfiguration.MachineStatistics != null)
            {
                dataContext.AddOrUpdate(vertimagConfiguration.MachineStatistics, d => d.Id);
            }

            dataContext.SaveChanges();

            this.Logger.LogInformation($"First run: configuration loaded.");

            await this.LoadSeedsAsync();

            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<IMachineProvider>().UpdateWeightStatistics(dataContext);
            }
        }

        private async Task LoadSeedsAsync()
        {
            using (var scope = this.ServiceScopeFactory.CreateScope())
            {
                var environment = scope.ServiceProvider.GetRequiredService<IHostingEnvironment>();
                var seedFileName = GetSeedFileName(environment.EnvironmentName);

                if (File.Exists(seedFileName))
                {
                    this.Logger.LogInformation($"First run: applying seed file '{seedFileName}' ...");

                    var seedScript = await File.ReadAllTextAsync(seedFileName);

                    var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();
                    await dataContext.Database.ExecuteSqlCommandAsync(seedScript);
                }
            }
        }

        #endregion
    }
}
