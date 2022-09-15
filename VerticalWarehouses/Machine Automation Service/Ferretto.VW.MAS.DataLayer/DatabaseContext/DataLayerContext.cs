using System;
using System.IO;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerContext : DbContext
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        private const string DefaultApplicationSettingsFile = "appsettings.json";

        private readonly IDbContextRedundancyService<DataLayerContext> dbContextRedundancy;

        #endregion

        #region Constructors

        public DataLayerContext()
        {
        }

        public DataLayerContext(DbContextOptions<DataLayerContext> options, IDbContextRedundancyService<DataLayerContext> dbContextRedundancy)
            : base(options)
        {
            this.Options = options ?? throw new ArgumentNullException(nameof(options));
            this.dbContextRedundancy = dbContextRedundancy ?? throw new ArgumentNullException(nameof(dbContextRedundancy));
        }

        #endregion

        #region Properties

        public DbSet<Accessory> Accessories { get; set; }

        public DbSet<AutoCompactingSettings> AutoCompactingSettings { get; set; }

        public DbSet<BayAccessories> BayAccessories { get; set; }

        public DbSet<BayPosition> BayPositions { get; set; }

        public DbSet<Bay> Bays { get; set; }

        public DbSet<CarouselManualParameters> CarouselManualParameters { get; set; }

        public DbSet<Carousel> Carousels { get; set; }

        public DbSet<CellPanel> CellPanels { get; set; }

        public DbSet<Cell> Cells { get; set; }

        public DbSet<DeviceInformation> DeviceInformation { get; set; }

        public DbSet<ElevatorAxis> ElevatorAxes { get; set; }

        public DbSet<ElevatorAxisManualParameters> ElevatorAxisManualParameters { get; set; }

        public DbSet<Elevator> Elevators { get; set; }

        public DbSet<ElevatorStructuralProperties> ElevatorStructuralProperties { get; set; }

        public DbSet<MachineError> Errors { get; set; }

        public DbSet<ErrorStatistic> ErrorStatistics { get; set; }

        public DbSet<External> Externals { get; set; }

        public DbSet<InstructionDefinition> InstructionDefinitions { get; set; }

        public DbSet<Instruction> Instructions { get; set; }

        public DbSet<InverterParameter> InverterParameter { get; set; }

        public DbSet<Inverter> Inverters { get; set; }

        public DbSet<IoDevice> IoDevices { get; set; }

        public DbSet<Laser> Lasers { get; set; }

        public DbSet<LoadingUnit> LoadingUnits { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<LogoutSettings> LogoutSettings { get; set; }

        public DbSet<Machine> Machines { get; set; }

        public DbSet<MachineStatistics> MachineStatistics { get; set; }

        public DbSet<Mission> Missions { get; set; }

        public DbSet<MovementParameters> MovementParameters { get; set; }

        public DbSet<MovementProfile> MovementProfiles { get; set; }

        public DbSet<RotationClassSchedule> RotationClassSchedule { get; set; }

        public DbSet<ServicingInfo> ServicingInfo { get; set; }

        public DbSet<SetupProcedure> SetupProcedures { get; set; }

        public DbSet<SetupProceduresSet> SetupProceduresSets { get; set; }

        public DbSet<ShutterManualParameters> ShutterManualParameters { get; set; }

        public DbSet<Shutter> Shutters { get; set; }

        public DbSet<TorqueCurrentMeasurementSession> TorqueCurrentMeasurementSessions { get; set; }

        public DbSet<TorqueCurrentSample> TorqueCurrentSamples { get; set; }

        public DbSet<UserParameters> Users { get; set; }

        public DbSet<WeightMeasurement> WeightMeasurements { get; set; }

        public DbSet<WmsSettings> WmsSettings { get; set; }

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
                connectionString = ((Microsoft.EntityFrameworkCore.Infrastructure.RelationalOptionsExtension)this.dbContextRedundancy.ActiveDbContextOptions.Extensions.First()).ConnectionString;
                //throw new InvalidOperationException($"Unable to locate the connection string '{ConnectionStringName}'.");
            }

            optionsBuilder
                .UseSqlite(connectionString)
                .EnableSensitiveDataLogging();
            //.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.QueryClientEvaluationWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }

        #endregion
    }
}
