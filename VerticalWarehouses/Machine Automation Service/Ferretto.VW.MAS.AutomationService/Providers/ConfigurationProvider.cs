using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class ConfigurationProvider : IConfigurationProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<ConfigurationProvider> logger;

        private readonly IMachineProvider machineProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ConfigurationProvider(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineProvider machineProvider,
            DataLayerContext dataContext,
            ILogger<ConfigurationProvider> logger)
        {
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public VertimagConfiguration ConfigurationGet()
        {
            this.logger.LogDebug($"Configuration Provider get");
            return new VertimagConfiguration
            {
                Machine = this.machineProvider.Get(),
                SetupProcedures = this.setupProceduresDataProvider.GetAll(),
                LoadingUnits = this.loadingUnitsDataProvider.GetAll(),
            };
        }

        public void ConfigurationImport(VertimagConfiguration vertimagConfiguration, IServiceScopeFactory serviceScopeFactory)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();
                using (var transaction = dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        this.machineProvider.Import(vertimagConfiguration.Machine, dataContext);

                        var loadingUnitsDataProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                        this.loadingUnitsDataProvider.Import(vertimagConfiguration.LoadingUnits, dataContext);

                        var setupProceduresDataProvider = scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
                        this.setupProceduresDataProvider.Import(vertimagConfiguration.SetupProcedures, dataContext);

                        dataContext.SaveChanges();

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

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dataContext = scope.ServiceProvider.GetRequiredService<DataLayerContext>();
                using (var transaction = dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        machineProvider.Update(vertimagConfiguration.Machine, dataContext);

                        var loadingUnitsDataProvider = scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
                        this.loadingUnitsDataProvider.UpdateRange(vertimagConfiguration.LoadingUnits, dataContext);

                        var setupProceduresDataProvider = scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
                        this.setupProceduresDataProvider.Update(vertimagConfiguration.SetupProcedures, dataContext);

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

        #endregion
    }
}
