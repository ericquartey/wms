using System;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer.DatabaseContext
{
    public partial class DataLayerContext : IRedundancyDbContext<DataLayerContext>
    {
        #region Fields

        private readonly IDbContextRedundancyService<DataLayerContext> redundancyService;

        #endregion

        #region Constructors

        public DataLayerContext(
            DbContextOptions<DataLayerContext> options,
            IDbContextRedundancyService<DataLayerContext> redundancyService)
            : this(options)
        {
            if (redundancyService == null)
            {
                throw new ArgumentNullException(nameof(redundancyService));
            }

            this.redundancyService = redundancyService;
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

            var affectedRecordsCount = 0;

            if (this.Options == this.redundancyService.ActiveDbContextOptions)
            {
                if (this.redundancyService.IsActiveDbInhibited)
                {
                    throw new DataLayerPersistentException(
                        DataLayerPersistentExceptionCode.PrimaryAndSecondaryPartitionFailure);
                }

                this.SaveToStandbyDb();
            }

            try
            {
                lock (this.redundancyService)
                {
                    affectedRecordsCount = base.SaveChanges();
                }
            }
            catch
            {
                if (this.Options == this.redundancyService.ActiveDbContextOptions)
                {
                    this.redundancyService.SwapContexts();
                }
                else
                {
                    this.redundancyService.InhibitStanbyDb();
                }
            }

            return affectedRecordsCount;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.redundancyService != null)
            {
                throw new NotSupportedException("Async save calls are not supported when redundancy is enabled.");
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SaveToStandbyDb()
        {
            if (this.redundancyService.IsStandbyDbInhibited)
            {
                return;
            }

            var standbyDbContext = new DataLayerContext(
                this.redundancyService.StandbyDbContextOptions,
                this.redundancyService);

            this.ChangeTracker.DetectChanges();

            try
            {
                foreach (var entry in this.ChangeTracker.Entries())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            standbyDbContext.Add(entry.Entity);
                            break;

                        case EntityState.Deleted:
                            standbyDbContext.Remove(entry.Entity);
                            break;

                        case EntityState.Modified:
                            standbyDbContext.Update(entry.Entity);
                            break;
                    }
                }

                lock (this.redundancyService)
                {
                    standbyDbContext.SaveChanges();
                }
            }
            catch
            {
                this.redundancyService.InhibitStanbyDb();
            }
        }

        #endregion
    }
}
