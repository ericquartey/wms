using System;
using System.IO;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal class AppBackupStep : Step
    {
        #region Constructors

        public AppBackupStep(int number, string title, string description, string appRootPath, string backupPath)
            : base(number, title, description)
        {
            this.AppRootPath = InterpolateVariables(appRootPath);
            this.BackupPath = InterpolateVariables(backupPath);

            if (!Path.IsPathRooted(this.AppRootPath))
            {
                throw new ArgumentException("The path must be rooted.", nameof(appRootPath));
            }

            if (!Path.IsPathRooted(this.BackupPath))
            {
                throw new ArgumentException("The path must be rooted.", nameof(backupPath));
            }
        }

        #endregion

        #region Properties

        public string AppRootPath { get; }

        public string BackupPath { get; }

        public int SubStep { get; set; }

        #endregion

        #region Methods

        protected override Task<StepStatus> OnApplyAsync()
        {
            try
            {
                this.SubStep = 1;
                if (Directory.Exists(this.BackupPath))
                {
                    this.LogInformation($"Eliminazione del percorso di destinazione '{this.BackupPath}' ...");
                    Directory.Delete(this.BackupPath, recursive: true);
                }

                this.SubStep = 2;

                this.LogInformation($"Spostamento della cartella sorgente '{this.AppRootPath}' nella cartella '{this.BackupPath}' ...");

                Directory.Move(this.AppRootPath, this.BackupPath);

                this.SubStep = 3;

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
                if (this.SubStep >= 2)
                {
                    if (Directory.Exists(this.BackupPath))
                    {
                        this.LogInformation($"Spostamento della cartella sorgente '{this.BackupPath}' nella cartella '{this.AppRootPath}' ...");

                        Directory.Move(this.BackupPath, Directory.GetParent(this.AppRootPath).FullName);
                    }
                    else
                    {
                        this.LogInformation($"Nulla da ripristinare in questo step.");
                    }
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
