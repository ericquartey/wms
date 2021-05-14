using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerContext : IRedundancyDbContext<DataLayerContext>
    {
        #region Fields

        private static readonly object SyncRoot = new object();

        private static int saveChangesCounter;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public DataLayerContext(
            bool isActiveChannel,
            IDbContextRedundancyService<DataLayerContext> redundancyService,
            ILogger<DataLayerContext> logger)
            : this(isActiveChannel ? redundancyService?.ActiveDbContextOptions : redundancyService?.StandbyDbContextOptions, redundancyService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.redundancyService = redundancyService ?? throw new ArgumentNullException(nameof(redundancyService));
        }

        #endregion

        #region Properties

        public DbContextOptions<DataLayerContext> Options { get; }

        #endregion

        #region Methods

        public override int SaveChanges()
        {
            if (this.redundancyService == null)
            {
                return base.SaveChanges();
            }

            var counter = saveChangesCounter++;

            var affectedRecordsCount = 0;
            lock (SyncRoot)
            {
                const int NUMBER_OF_RETRIES = 4;
                for (var i = 0; i < NUMBER_OF_RETRIES; i++)
                {
                    try
                    {
                        affectedRecordsCount = base.SaveChanges();
                        break;
                    }
                    catch (Microsoft.Data.Sqlite.SqliteException ex) when (i < NUMBER_OF_RETRIES - 1)
                    {
                        this.logger.LogDebug($"Try: #{i + 1}. Error reason: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogDebug($"Try: #{i + 1}. Error reason ex: {ex.Message}");
                        // Swap is handled in diagnostic interceptor:
                        // Microsoft.EntityFrameworkCore.Database.Command.CommandError
                        System.Diagnostics.Debug.Assert(
                              this.Options == this.redundancyService.StandbyDbContextOptions,
                              $"This channel (previously was active) is now standby because {ex.Message}");

                        if (this.redundancyService.IsActiveDbInhibited)
                        {
                            throw new DataLayerPersistentException(
                                DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
                        }

                        affectedRecordsCount = this.SaveToActiveDb();
                        break;
                    }
                }
            }

            return affectedRecordsCount;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (this.redundancyService != null)
            {
                throw new NotSupportedException("Async save calls are not supported when redundancy is enabled.");
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        public override string ToString()
        {
            if (this.redundancyService == null)
            {
                return base.ToString();
            }

            return this.redundancyService.ActiveDbContextOptions == this.Options
                ? "Active DataLayer channel"
                : "Standby DataLayer channel";
        }

        private int SaveToActiveDb()
        {
            using (var dbContext = new DataLayerContext(isActiveChannel: true, this.redundancyService, this.logger))
            {
                var affectedRecordsCount = 0;
                try
                {
                    affectedRecordsCount = dbContext.SaveChanges();
                }
                catch
                {
                    // Do nothing.
                    // Errors are handled in diagnostic interceptor:
                    // Microsoft.EntityFrameworkCore.Database.Command.CommandError
                }

                return affectedRecordsCount;
            }
        }

        #endregion
    }
}
