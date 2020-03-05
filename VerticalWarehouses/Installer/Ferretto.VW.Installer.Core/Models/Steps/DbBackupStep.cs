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

        private string primaryDbPath;

        private string secondaryDbPath;

        #endregion

        #region Constructors

        public DbBackupStep(int number, string title, string description, string automationServicePath, string backupPath, string log, MachineRole machineRole, SetupMode setupMode, bool skipOnResume)
                            : base(number, title, description, log, machineRole, setupMode, skipOnResume)
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
                this.LogInformation($"Loading settings from '{appSettingsPath}' ...");

                var appSettingsString = File.ReadAllText(appSettingsPath);
                var appSettings = JsonConvert.DeserializeObject<AppSettings>(appSettingsString);

                Directory.CreateDirectory(this.BackupPath);

                this.primaryDbPath = appSettings.ConnectionStrings.PrimaryDbPath;
                this.secondaryDbPath = appSettings.ConnectionStrings.SecondaryDbPath;

                if (File.Exists(appSettingsProductionPath))
                {
                    this.LogInformation($"Found additional settings in '{appSettingsProductionPath}'.");

                    this.LogError($"This configuration is not supported. Please manually merge the two files.");

                    return Task.FromResult(StepStatus.Failed);
                }

                if (!Path.IsPathRooted(this.primaryDbPath))
                {
                    this.primaryDbPath = Path.Combine(this.AutomationServicePath, this.primaryDbPath);
                }
                this.LogInformation($"Backing up database file '{this.primaryDbPath}' ...");
                File.Copy(this.primaryDbPath, Path.Combine(this.BackupPath, PrimaryDbName), true);

                if (!Path.IsPathRooted(this.secondaryDbPath))
                {
                    this.secondaryDbPath = Path.Combine(this.AutomationServicePath, this.secondaryDbPath);
                }
                this.LogInformation($"Backing up database file '{this.secondaryDbPath}' ...");
                File.Copy(this.secondaryDbPath, Path.Combine(this.BackupPath, SecondaryDbName), true);

                return Task.FromResult(StepStatus.Done);
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
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
                this.LogError(ex.Message);
                return Task.FromResult(StepStatus.RollbackFailed);
            }
        }

        #endregion
    }
}
