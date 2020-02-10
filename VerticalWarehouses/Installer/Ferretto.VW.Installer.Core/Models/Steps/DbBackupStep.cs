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

        #endregion

        #region Constructors

        public DbBackupStep(int number, string title, string description, string automationServicePath, string backupPath)
            : base(number, title, description)
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

                var primaryDbPath = appSettings.ConnectionStrings.PrimaryDbPath;
                var secondaryDbPath = appSettings.ConnectionStrings.SecondaryDbPath;

                if (File.Exists(appSettingsProductionPath))
                {
                    this.LogInformation($"Found additional settings in '{appSettingsProductionPath}'.");

                    this.LogError($"This configuration is not supported. Please manually merge the two files.");

                    return Task.FromResult(StepStatus.Failed);
                }

                if (!Path.IsPathRooted(primaryDbPath))
                {
                    primaryDbPath = Path.Combine(this.AutomationServicePath, primaryDbPath);
                }
                this.LogInformation($"Backing up database file '{primaryDbPath}' ...");
                File.Copy(primaryDbPath, Path.Combine(this.BackupPath, PrimaryDbName), true);

                if (!Path.IsPathRooted(secondaryDbPath))
                {
                    secondaryDbPath = Path.Combine(this.AutomationServicePath, secondaryDbPath);
                }
                this.LogInformation($"Backing up database file '{secondaryDbPath}' ...");
                File.Copy(secondaryDbPath, Path.Combine(this.BackupPath, SecondaryDbName), true);

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
                File.Delete(Path.Combine(this.BackupPath, PrimaryDbName));
                File.Delete(Path.Combine(this.BackupPath, SecondaryDbName));

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
