using System;
using System.IO;
using System.Linq;
using Ferretto.VW.CommonUtils.Converters;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class ConfigurationProvider : IConfigurationProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly DataLayerContext dataContext;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ConfigurationProvider> logger;

        private readonly IMachineProvider machineProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public ConfigurationProvider(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineProvider machineProvider,
            IWmsSettingsProvider wmsSettingsProvider,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory,
            DataLayerContext dataContext,
            ILogger<ConfigurationProvider> logger)
        {
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.baysDataProvider = baysDataProvider;
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public VertimagConfiguration ConfigurationGet()
        {
            return new VertimagConfiguration
            {
                Machine = this.machineProvider.Get(),
                SetupProcedures = this.setupProceduresDataProvider.GetAll(),
                LoadingUnits = this.loadingUnitsDataProvider.GetAll(),
                MachineStatistics = this.machineProvider.GetStatistics(),
                ServicingInfo = this.machineProvider.GetServicingInfo(),
                Wms = this.wmsSettingsProvider.GetAll()
            };
        }

        public void ConfigurationImport(VertimagConfiguration vertimagConfiguration, IServiceScopeFactory serviceScopeFactory)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));

            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.machineProvider.Import(vertimagConfiguration.Machine, this.dataContext);
                        this.loadingUnitsDataProvider.Import(vertimagConfiguration.LoadingUnits, this.dataContext);
                        this.setupProceduresDataProvider.Import(vertimagConfiguration.SetupProcedures, this.dataContext);
                        this.machineProvider.ImportMachineStatistics(vertimagConfiguration.MachineStatistics, this.dataContext);
                        this.machineProvider.ImportMachineServicingInfo(vertimagConfiguration.ServicingInfo, this.dataContext);

                        this.dataContext.SaveChanges();

                        transaction.Commit();
                        this.logger.LogInformation($"Configuration Provider import");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        this.logger.LogError(e, $"Configuration Provider import exception");
                    }
                }
            }
        }

        public void ConfigurationUpdate(VertimagConfiguration vertimagConfiguration, IServiceScopeFactory serviceScopeFactory)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));

            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.machineProvider.Update(vertimagConfiguration.Machine, this.dataContext);
                        this.loadingUnitsDataProvider.UpdateRange(vertimagConfiguration.LoadingUnits, this.dataContext);
                        this.setupProceduresDataProvider.Update(vertimagConfiguration.SetupProcedures, this.dataContext);
                        this.machineProvider.UpdateMachineStatistics(vertimagConfiguration.MachineStatistics.LastOrDefault(), this.dataContext);
                        if (vertimagConfiguration.ServicingInfo.Any())
                        {
                            this.machineProvider.UpdateMachineServicingInfo(vertimagConfiguration.ServicingInfo.LastOrDefault(), this.dataContext);
                        }

                        transaction.Commit();
                        this.logger.LogInformation($"Configuration Provider update");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        this.logger.LogError(e, $"Configuration Provider update exception");
                    }
                }
            }
        }

        public void UpdateMachine(Machine machine)
        {
            _ = machine ?? throw new ArgumentNullException(nameof(machine));

            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        //this.machineProvider.Update(machine, this.dataContext);
                        this.machineProvider.UpdateSolo(machine, this.dataContext);

                        transaction.Commit();
                        this.logger.LogInformation($"Configuration Provider update");
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        this.logger.LogError(e, $"Configuration Provider update exception");
                    }
                }
            }
        }

        public VertimagConfiguration GetJsonConfiguration()
        {
            try
            {
                var scope = this.serviceScopeFactory.CreateScope();

                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var ConfigurationFile = configuration.GetValue<string>("Vertimag:DataLayer:ConfigurationFile");

                if (string.IsNullOrEmpty(ConfigurationFile))
                {
                    return null;
                }

                this.logger.LogInformation($"Run: loading configuration from JSON file ({ConfigurationFile}) ...");

                string fileContents;
                using (var streamReader = new StreamReader(ConfigurationFile))
                {
                    fileContents = streamReader.ReadToEnd();
                }

                var jsonObject = JObject.Parse(fileContents);

                ValidateJson(jsonObject);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());

                var vertimagConfiguration = JsonConvert.DeserializeObject<VertimagConfiguration>(jsonObject.ToString(), settings);
                vertimagConfiguration.Validate();

                return vertimagConfiguration;
            }
            catch (Exception e)
            {
                this.logger.LogError($"Exeption While loading configuration from JSON file ({e.Message}) ...");

                return null;
            }
        }

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
            }
        }

        #endregion
    }
}
