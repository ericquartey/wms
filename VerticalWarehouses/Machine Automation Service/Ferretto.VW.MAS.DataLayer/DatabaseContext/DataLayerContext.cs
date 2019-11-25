using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.Configurations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerContext : DbContext
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        #endregion

        #region Constructors

        public DataLayerContext()
        {
        }

        public DataLayerContext(DbContextOptions<DataLayerContext> options)
            : base(options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #endregion

        #region Properties

        public DbSet<BayPosition> BayPositions { get; set; }

        public DbSet<Bay> Bays { get; set; }

        public DbSet<CarouselManualParameters> CarouselManualParameters { get; set; }

        public DbSet<Carousel> Carousels { get; set; }

        public DbSet<CellPanel> CellPanels { get; set; }

        public DbSet<Cell> Cells { get; set; }

        public DbSet<ElevatorAxis> ElevatorAxes { get; set; }

        public DbSet<ElevatorAxisManualParameters> ElevatorAxisManualParameters { get; set; }

        public DbSet<Elevator> Elevators { get; set; }

        public DbSet<ElevatorStructuralProperties> ElevatorStructuralProperties { get; set; }

        public DbSet<ErrorDefinition> ErrorDefinitions { get; set; }

        public DbSet<MachineError> Errors { get; set; }

        public DbSet<ErrorStatistic> ErrorStatistics { get; set; }

        public DbSet<Inverter> Inverters { get; set; }

        public DbSet<IoDevice> IoDevices { get; set; }

        public DbSet<LoadingUnit> LoadingUnits { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MachineStatistics> MachineStatistics { get; set; }

        public DbSet<MovementParameters> MovementParameters { get; set; }

        public DbSet<MovementProfile> MovementProfiles { get; set; }

        public DbSet<ServicingInfo> ServicingInfo { get; set; }

        public DbSet<SetupProcedure> SetupProcedures { get; set; }

        public DbSet<SetupProceduresSet> SetupProceduresSets { get; set; }

        public DbSet<SetupStatus> SetupStatus { get; set; }

        public DbSet<ShutterManualParameters> ShutterManualParameters { get; set; }

        public DbSet<Shutter> Shutters { get; set; }

        public DbSet<TorqueCurrentMeasurementSession> TorqueCurrentMeasurementSessions { get; set; }

        public DbSet<TorqueCurrentSample> TorqueCurrentSamples { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<WeightMeasurement> WeightMeasurements { get; set; }

        #endregion

        #region Methods

        public override void Dispose()
        {
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _ = optionsBuilder ?? throw new ArgumentNullException(nameof(optionsBuilder));

            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(DefaultApplicationSettingsFile, optional: false, reloadOnChange: false)
                .Build();

            var connectionString = configurationBuilder.GetConnectionString(ConnectionStringName);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Unable to locate the connection string '{ConnectionStringName}'.");
            }

            optionsBuilder
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder
                .ApplyConfiguration(new BaysConfiguration())
                .ApplyConfiguration(new BayPositionsConfiguration())
                .ApplyConfiguration(new PanelsConfiguration())
                .ApplyConfiguration(new CellsConfiguration())
                .ApplyConfiguration(new TorqueCurrentSampleConfiguration())
                .ApplyConfiguration(new ElevatorAxisManualParametersConfiguration())
                .ApplyConfiguration(new CarouselManualParametersConfiguration())
                .ApplyConfiguration(new ErrorDefinitionConfiguration())
                .ApplyConfiguration(new ErrorConfiguration())
                .ApplyConfiguration(new ErrorStatisticConfiguration())
                .ApplyConfiguration(new InvertersConfiguration())
                .ApplyConfiguration(new IoDevicesConfiguration())
                .ApplyConfiguration(new LoadingUnitsConfiguration())
                .ApplyConfiguration(new MachineStatisticsConfiguration())
                .ApplyConfiguration(new MovementProfilesConfiguration())
                .ApplyConfiguration(new ServicingInfoConfiguration())
                .ApplyConfiguration(new SetupStatusConfiguration())
                .ApplyConfiguration(new ShutterManualParametersConfiguration())
                .ApplyConfiguration(new ShuttersConfiguration())
                .ApplyConfiguration(new TorqueCurrentMeasurementSessionsConfiguration())
                .ApplyConfiguration(new UsersConfiguration());
        }

        #endregion
    }
}
