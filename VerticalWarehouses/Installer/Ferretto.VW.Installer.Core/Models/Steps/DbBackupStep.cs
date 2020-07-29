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

        public DbBackupStep(
            int number,
            string title,
            string description,
            string automationServicePath,
            string telemetryDatabaseRoot,
            string telemetryDatabaseFolder,
            string backupPath,
            MachineRole machineRole,
            SetupMode setupMode,
            bool skipOnResume,
            bool skipRollback)
            : base(number, title, description, machineRole, setupMode, skipOnResume, skipRollback)
        {
            this.AutomationServicePath = InterpolateVariables(automationServicePath);
            this.TelemetryDatabaseRoot = InterpolateVariables(telemetryDatabaseRoot);
            this.TelemetryDatabaseFolder = InterpolateVariables(telemetryDatabaseFolder);
            this.BackupPath = InterpolateVariables(backupPath);
        }

        #endregion

        #region Properties
        public string TelemetryDatabaseRoot { get; }
        public string AutomationServicePath { get; }

        public string TelemetryDatabaseFolder { get; }
        

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

                var telemetryFolderPath = Path.Combine(this.TelemetryDatabaseRoot, this.TelemetryDatabaseFolder);
                this.Execution.LogInformation($"Backing up database telemetry '{telemetryFolderPath}' ...");                
                if (Directory.Exists(telemetryFolderPath))
                {
                    this.DirectoryCopy(telemetryFolderPath, Path.Combine(this.BackupPath, this.TelemetryDatabaseFolder));                    
                }
                return Task.FromResult(StepStatus.Done);
            }
            catch (Exception ex)
            {
                this.Execution.LogError(ex.Message);
                return Task.FromResult(StepStatus.Failed);
            }
        }

        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                var msg = $"Source directory does not exist or could not be found: {sourceDirName}";
                this.Execution.LogError(msg);
                throw new DirectoryNotFoundException(msg);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    this.DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
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
