using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

// ReSharper disable ArrangeThisQualifier
// ReSharper disable ParameterHidesMember
namespace Ferretto.VW.MAS.DataLayer
{
    internal partial class DataLayerService
    {
        #region Methods

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
                fileContents = streamReader.ReadToEnd();
            }

            var jsonObject = JObject.Parse(fileContents);

            var schema = JSchema.Load(new JsonTextReader(new StreamReader("configuration/schemas/vertimag-configuration-schema.json")));
            jsonObject.Validate(schema);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new IPAddressConverter());

            var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);

            dataContext.Machines.Add(vertimagConfiguration.Machine);
            dataContext.LoadingUnits.AddRange(vertimagConfiguration.LoadingUnits);
            dataContext.SetupProceduresSets.Add(vertimagConfiguration.SetupProcedures);

            dataContext.SaveChanges();

            this.Logger.LogInformation($"First run: configuration loaded.");

            await this.LoadSeedsAsync();
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
