using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ferretto.VW.Installer.Core
{
    internal class DbBackupStep : Step
    {
        #region Fields

        private const string PrimaryDbName = "primary.db";

        private const string SecondaryDbName = "secondary.db";

        private const string TelemetryFirstDbName = "telemetry.realm";

        private const string TelemetrySecondDbName = "telemetry.realm.lock";

        private string primaryDbPath;

        private string secondaryDbPath;

        private string telemetryFirstDbPath;

        private string telemetrySecondDbPath;

        #endregion

        #region Constructors

        public DbBackupStep(
            int number,
            string title,
            string description,
            string automationServicePath,
            string backupPath,
            MachineRole machineRole,
            SetupMode setupMode,
            bool skipOnResume,
            bool skipRollback)
            : base(number, title, description, machineRole, setupMode, skipOnResume, skipRollback)
        {
            this.AutomationServicePath = InterpolateVariables(automationServicePath);
            this.BackupPath = InterpolateVariables(backupPath);
        }

        #endregion

        #region Properties

        public string AutomationServicePath { get; }

        public string BackupPath { get; }

        #endregion

        #region Methods

        protected override Task<StepStatus> OnApplyAsync()
        {
            var appSettingsPath = Path.Combine(this.AutomationServicePath, "appsettings.json");
            var appSettingsProductionPath = Path.Combine(this.AutomationServicePath, "appsettings.production.json");

            try
            {
                this.Execution.LogInformation($"Loading settings from '{appSettingsPath}' ...");

                var appSettingsString = File.ReadAllText(appSettingsPath);
                var appSettings = JsonConvert.DeserializeObject<AppSettings>(appSettingsString);

                Directory.CreateDirectory(this.BackupPath);

                this.primaryDbPath = appSettings.ConnectionStrings.PrimaryDbPath;
                this.secondaryDbPath = appSettings.ConnectionStrings.SecondaryDbPath;
                this.telemetryFirstDbPath = this.secondaryDbPath;
                this.telemetryFirstDbPath = this.secondaryDbPath;

                if (File.Exists(appSettingsProductionPath))
                {
                    this.Execution.LogInformation($"Found additional settings in '{appSettingsProductionPath}'.");

                    this.Execution.LogError($"This configuration is not supported. Please manually merge the two files.");

                    return Task.FromResult(StepStatus.Failed);
                }

                if (!Path.IsPathRooted(this.primaryDbPath))
                {
                    this.primaryDbPath = Path.Combine(this.AutomationServicePath, this.primaryDbPath);
                }
                this.Execution.LogInformation($"Backing up database file '{this.primaryDbPath}' ...");
                File.Copy(this.primaryDbPath, Path.Combine(this.BackupPath, PrimaryDbName), true);

                if (!Path.IsPathRooted(this.secondaryDbPath))
                {
                    this.secondaryDbPath = Path.Combine(this.AutomationServicePath, this.secondaryDbPath);
                }
                this.Execution.LogInformation($"Backing up database file '{this.secondaryDbPath}' ...");
                File.Copy(this.secondaryDbPath, Path.Combine(this.BackupPath, SecondaryDbName), true);

                if (!Path.IsPathRooted(this.telemetryFirstDbPath))
                {
                    this.telemetryFirstDbPath = Path.Combine(this.AutomationServicePath, this.telemetryFirstDbPath);
                }
                this.Execution.LogInformation($"Backing up database file '{this.telemetryFirstDbPath}\\{TelemetryFirstDbName}' ...");
                File.Copy(this.telemetryFirstDbPath, Path.Combine(this.BackupPath, TelemetryFirstDbName), true);

                this.Execution.LogInformation($"Backing up database file '{this.telemetryFirstDbPath}\\{TelemetrySecondDbName}' ...");
                File.Copy(this.telemetryFirstDbPath, Path.Combine(this.BackupPath, TelemetrySecondDbName), true);
                return Task.FromResult(StepStatus.Done);
            }
            catch (Exception ex)
            {
                this.Execution.LogError(ex.Message);
                return Task.FromResult(StepStatus.Failed);
            }
        }

        protected override Task<StepStatus> OnRollbackAsync()
        {
            try
            {
                if (this.SetupMode == SetupMode.Restore)
                {
                    File.Copy(Path.Combine(this.BackupPath, PrimaryDbName), this.primaryDbPath, true);
                    File.Copy(Path.Combine(this.BackupPath, SecondaryDbName), this.secondaryDbPath, true);
                }
                else
                {
                    File.Delete(Path.Combine(this.BackupPath, PrimaryDbName));
                    File.Delete(Path.Combine(this.BackupPath, SecondaryDbName));
                }

                return Task.FromResult(StepStatus.RolledBack);
            }
            catch (Exception ex)
            {
                this.Execution.LogError(ex.Message);
                return Task.FromResult(StepStatus.RollbackFailed);
            }
        }

        #endregion
    }
}
