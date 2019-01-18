using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.Common.DataModels;
using Ferretto.Common.EF.Configurations;
using Microsoft.EntityFrameworkCore;

#if NET4
using System.Configuration;
#else
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
#endif

namespace Ferretto.Common.EF
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Major Code Smell",
        "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)",
        Justification = "Class Designed as part of the Entity Framework")]
    public class DatabaseContext : DbContext
    {
        protected const string ConnectionStringName = "WmsConnectionString";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        private const string ParametrizedApplicationSettingsFile = "appsettings.{0}.json";

        private const string NetcoreEnvironmentEnvVariable = "NETCORE_ENVIRONMENT";

        #region Constructors

        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        #endregion Constructors

        #region Properties

        public virtual DbSet<AbcClass> AbcClasses { get; set; }

        public virtual DbSet<Aisle> Aisles { get; set; }

        public virtual DbSet<Area> Areas { get; set; }

        public virtual DbSet<Bay> Bays { get; set; }

        public virtual DbSet<BayType> BayTypes { get; set; }

        public virtual DbSet<CellConfigurationCellPositionLoadingUnitType> CellConfigurationCellPositionLoadingUnitTypes
        {
            get;
            set;
        }

        public virtual DbSet<CellConfigurationCellType> CellConfigurationCellTypes { get; set; }

        public virtual DbSet<Common.DataModels.CellConfiguration> CellConfigurations { get; set; }

        public virtual DbSet<CellHeightClass> CellHeightClasses { get; set; }

        public virtual DbSet<CellPosition> CellPositions { get; set; }

        public virtual DbSet<Cell> Cells { get; set; }

        public virtual DbSet<CellSizeClass> CellSizeClasses { get; set; }

        public virtual DbSet<CellStatus> CellStatuses { get; set; }

        public virtual DbSet<CellTotal> CellTotals { get; set; }

        public virtual DbSet<CellType> CellTypes { get; set; }

        public virtual DbSet<CellTypeAisle> CellTypesAisles { get; set; }

        public virtual DbSet<CellWeightClass> CellWeightClasses { get; set; }

        public virtual DbSet<Compartment> Compartments { get; set; }

        public virtual DbSet<CompartmentStatus> CompartmentStatuses { get; set; }

        public virtual DbSet<CompartmentType> CompartmentTypes { get; set; }

        public virtual DbSet<DefaultCompartment> DefaultCompartments { get; set; }

        public virtual DbSet<DefaultLoadingUnit> DefaultLoadingUnits { get; set; }

        public virtual DbSet<ItemCategory> ItemCategories { get; set; }

        public virtual DbSet<ItemListRow> ItemListRows { get; set; }

        public virtual DbSet<ItemList> ItemLists { get; set; }

        public virtual DbSet<Item> Items { get; set; }

        public virtual DbSet<ItemArea> ItemsAreas { get; set; }

        public virtual DbSet<ItemCompartmentType> ItemsCompartmentTypes { get; set; }

        public virtual DbSet<LoadingUnitHeightClass> LoadingUnitHeightClasses { get; set; }

        public virtual DbSet<LoadingUnitRange> LoadingUnitRanges { get; set; }

        public virtual DbSet<LoadingUnit> LoadingUnits { get; set; }

        public virtual DbSet<LoadingUnitSizeClass> LoadingUnitSizeClasses { get; set; }

        public virtual DbSet<LoadingUnitStatus> LoadingUnitStatuses { get; set; }

        public virtual DbSet<LoadingUnitType> LoadingUnitTypes { get; set; }

        public virtual DbSet<LoadingUnitTypeAisle> LoadingUnitTypesAisles { get; set; }

        public virtual DbSet<LoadingUnitWeightClass> LoadingUnitWeightClasses { get; set; }

        public virtual DbSet<Machine> Machines { get; set; }

        public virtual DbSet<MachineType> MachineTypes { get; set; }

        public virtual DbSet<MaterialStatus> MaterialStatuses { get; set; }

        public virtual DbSet<MeasureUnit> MeasureUnits { get; set; }

        public virtual DbSet<Mission> Missions { get; set; }

        public virtual DbSet<PackageType> PackageTypes { get; set; }

        public virtual DbSet<SchedulerRequest> SchedulerRequests { get; set; }

        #endregion Properties

        #region Methods

        public override int SaveChanges()
        {
            this.AddTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            this.AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected static string GetSettingFileFromEnvironment()
        {
            var netcoreEnvironment = Environment.GetEnvironmentVariable(NetcoreEnvironmentEnvVariable);

            return netcoreEnvironment != null
                       ? string.Format(ParametrizedApplicationSettingsFile, netcoreEnvironment)
                       : DefaultApplicationSettingsFile;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(optionsBuilder));
            }

            if (optionsBuilder.IsConfigured)
            {
                return;
            }

#if NET4
            var configuration = ConfigurationManager.ConnectionStrings[ConnectionStringName];

            if (configuration == null)
            {
                throw new ConfigurationErrorsException($"Connection string '{ConnectionStringName}' not found.");
            }

            optionsBuilder.UseSqlServer(configuration.ConnectionString);
#else
            var applicationSettingsFile = GetSettingFileFromEnvironment();

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(applicationSettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);

            optionsBuilder.UseSqlServer(connectionString);
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfiguration(new AbcClassConfiguration());
            modelBuilder.ApplyConfiguration(new AisleConfiguration());
            modelBuilder.ApplyConfiguration(new AreaConfiguration());
            modelBuilder.ApplyConfiguration(new BayConfiguration());
            modelBuilder.ApplyConfiguration(new BayTypeConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.CellConfiguration());
            modelBuilder.ApplyConfiguration(new CellConfigurationConfiguration());
            modelBuilder.ApplyConfiguration(new CellConfigurationCellPositionLoadingUnitTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CellConfigurationCellTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CellHeightClassConfiguration());
            modelBuilder.ApplyConfiguration(new CellPositionConfiguration());
            modelBuilder.ApplyConfiguration(new CellSizeClassConfiguration());
            modelBuilder.ApplyConfiguration(new CellStatusConfiguration());
            modelBuilder.ApplyConfiguration(new CellTotalConfiguration());
            modelBuilder.ApplyConfiguration(new CellTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CellTypeAisleConfiguration());
            modelBuilder.ApplyConfiguration(new CellWeightClassConfiguration());
            modelBuilder.ApplyConfiguration(new CompartmentConfiguration());
            modelBuilder.ApplyConfiguration(new CompartmentStatusConfiguration());
            modelBuilder.ApplyConfiguration(new CompartmentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DefaultCompartmentConfiguration());
            modelBuilder.ApplyConfiguration(new DefaultLoadingUnitConfiguration());
            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new ItemAreaConfiguration());
            modelBuilder.ApplyConfiguration(new ItemCompartmentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ItemListConfiguration());
            modelBuilder.ApplyConfiguration(new ItemListRowConfiguration());
            modelBuilder.ApplyConfiguration(new ItemCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitHeightClassConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitRangeConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitSizeClassConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitStatusConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitTypeConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitTypeAisleConfiguration());
            modelBuilder.ApplyConfiguration(new LoadingUnitWeightClassConfiguration());
            modelBuilder.ApplyConfiguration(new MachineConfiguration());
            modelBuilder.ApplyConfiguration(new MachineTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaterialStatusConfiguration());
            modelBuilder.ApplyConfiguration(new MeasureUnitConfiguration());
            modelBuilder.ApplyConfiguration(new MissionConfiguration());
            modelBuilder.ApplyConfiguration(new PackageTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SchedulerRequestConfiguration());
        }

        private void AddTimestamps()
        {
            var entries = this.ChangeTracker.Entries()
                .Where(x =>
                    x.Entity is ITimestamped
                    &&
                    (x.State == EntityState.Added || x.State == EntityState.Modified));

            var timeNow = System.DateTime.UtcNow;

            foreach (var entry in entries)
            {
                var timestampedEntity = (ITimestamped)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    timestampedEntity.CreationDate = timeNow;
                }

                timestampedEntity.LastModificationDate = timeNow;
            }
        }

        #endregion Methods
    }
}
