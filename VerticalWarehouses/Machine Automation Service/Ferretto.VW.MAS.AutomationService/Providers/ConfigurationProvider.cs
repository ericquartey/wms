using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class ConfigurationProvider : IConfigurationProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILoadingUnitsProvider loadingUnitsProvider;

        private readonly ILogger<ConfigurationProvider> logger;

        private readonly IMachineProvider machineProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ConfigurationProvider(
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILoadingUnitsProvider loadingUnitsProvider,
            IMachineProvider machineProvider,
            DataLayerContext dataContext,
            ILogger<ConfigurationProvider> logger)
        {
            this.loadingUnitsProvider = loadingUnitsProvider ?? throw new ArgumentNullException(nameof(loadingUnitsProvider));
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
                LoadingUnits = this.loadingUnitsProvider.GetAll(),
            };
        }

        public void ConfigurationImport(VertimagConfiguration vertimagConfiguration)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));

            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.machineProvider.Import(vertimagConfiguration.Machine, this.dataContext);
                        this.loadingUnitsProvider.Import(vertimagConfiguration.LoadingUnits, this.dataContext);
                        this.setupProceduresDataProvider.Import(vertimagConfiguration.SetupProcedures, this.dataContext);

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

        public void ConfigurationUpdate(VertimagConfiguration vertimagConfiguration)
        {
            _ = vertimagConfiguration ?? throw new ArgumentNullException(nameof(vertimagConfiguration));

            lock (this.dataContext)
            {
                using (var transaction = this.dataContext.Database.BeginTransaction())
                {
                    try
                    {
                        this.machineProvider.Update(vertimagConfiguration.Machine);
                        this.loadingUnitsProvider.UpdateRange(vertimagConfiguration.LoadingUnits);
                        this.setupProceduresDataProvider.Update(vertimagConfiguration.SetupProcedures);

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
