using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    internal sealed class InstallationService : BindableBase, IInstallationService
    {
        #region Fields

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ISetupModeService setupModeService;

        private Step? activeStep;

        private bool isRollbackInProgress;

        private bool isStarted;

        private MachineRole machineRole;

        private Uri? masUrl;

        private string? masVersion;

        private string? panelPcVersion;

        private IPAddress? ppcIpAddress;

        private string? softwareVersion;

        private Uri? tsUrl;

        #endregion

        #region Constructors

        public InstallationService(ISetupModeService setupModeService)
        {
            if (setupModeService is null)
            {
                throw new ArgumentNullException(nameof(setupModeService));
            }

            this.setupModeService = setupModeService;
        }

        #endregion

        #region Events

        public event EventHandler<InstallationFinishedEventArgs>? Finished;

        #endregion

        #region Properties

        public Step? ActiveStep
        {
            get => this.activeStep;
            private set => this.SetProperty(ref this.activeStep, value);
        }

        public string? InstallerVersion
        {
            get => this.softwareVersion;
            private set => this.SetProperty(ref this.softwareVersion, value);
        }

        public bool IsRollbackInProgress
        {
            get => this.isRollbackInProgress;
            private set => this.SetProperty(ref this.isRollbackInProgress, value, () => this.logger.Debug($"IsRollbackInProgress = {value}"));
        }

        public MachineRole MachineRole
        {
            get => this.machineRole;
            private set => this.SetProperty(ref this.machineRole, value);
        }

        public Uri? MasUrl => this.masUrl;

        public string? MasVersion
        {
            get => this.masVersion;
            private set => this.SetProperty(ref this.masVersion, value);
        }

        public string? PanelPcVersion
        {
            get => this.panelPcVersion;
            private set => this.SetProperty(ref this.panelPcVersion, value);
        }

        public IEnumerable<Step> Steps { get; private set; } = new List<Step>();

        public Uri? TsUrl => this.tsUrl;

        #endregion

        #region Methods

        public void Abort()
        {
            if (!this.isStarted)
            {
                throw new InvalidOperationException();
            }

            this.IsRollbackInProgress = true;
        }

        public bool CanStart()
        {
            System.Diagnostics.Debug.Assert(this.setupModeService.Mode != SetupMode.Any);

            if (this.Steps.All(s => s.Execution.StartTime.HasValue))
            {
                this.logger.Debug("Nothing to do, update/installation in progress.");
                return false;
            }

            return true;
        }

        public async Task DeserializeAsync(string sourceFileName)
        {
            if (this.isStarted)
            {
                throw new InvalidOperationException();
            }

            this.GetInstallerParameters();

            this.InstallerVersion = (await AppManifest.FromFileAsync("./properties/app.manifest"))?.AssemblyIdentityVersion;
            this.PanelPcVersion = (await AppManifest.FromFileAsync("../automation_service/properties/app.manifest"))?.AssemblyIdentityVersion;
            this.MasVersion = (await AppManifest.FromFileAsync("../panelpc_app/properties/app.manifest"))?.AssemblyIdentityVersion;

            await this.LoadStepsAsync(sourceFileName);
        }

        public void GetInstallerParameters()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var masUrlString = config.AppSettings.Settings["MAS:BaseUrl"].Value;
                if (Uri.TryCreate(masUrlString, UriKind.Absolute, out var masUrl))
                {
                    this.masUrl = masUrl;
                }
                else
                {
                    this.masUrl = null;
                }

                var tsUrlString = config.AppSettings.Settings["TS:BaseUrl"].Value;
                if (Uri.TryCreate(tsUrlString, UriKind.Absolute, out var tsUrl))
                {
                    this.tsUrl = tsUrl;
                }
                else
                {
                    this.tsUrl = null;
                }

                var ppcIpAddressString = config.AppSettings.Settings["Install:Parameter:PpcIpAddress"].Value;
                if (IPAddress.TryParse(ppcIpAddressString, out var ppcIpAddress))
                {
                    this.ppcIpAddress = ppcIpAddress;
                }
                else
                {
                    this.ppcIpAddress = null;
                }

                this.UpdateMachineRole();
            }
            catch (Exception)
            {
                this.masUrl = null;
                this.tsUrl = null;
                this.ppcIpAddress = null;
            }
        }

        public async Task LoadStepsAsync(string sourceFileName)
        {
            this.logger.Debug($"Loading execution steps from '{Path.Combine(Directory.GetCurrentDirectory(), sourceFileName)}' ...");

            try
            {
                var jsonFile = await File.ReadAllTextAsync(sourceFileName);
                var serviceSnapshot = new
                {
                    Steps = Array.Empty<Step>(),
                    IsRollbackInProgress = false,
                    SetupMode = this.setupModeService.Mode,
                    ActiveStep = (Step?)null,
                };

                serviceSnapshot = JsonConvert.DeserializeAnonymousType(
                    jsonFile,
                    serviceSnapshot,
                    SerializerSettings);

                this.setupModeService.Mode = serviceSnapshot.SetupMode != SetupMode.Any
                    ? serviceSnapshot.SetupMode
                    : this.setupModeService.Mode;

                this.IsRollbackInProgress = serviceSnapshot.IsRollbackInProgress;

                var mode = this.setupModeService.Mode;
                this.Steps = serviceSnapshot.Steps
                    .Where(s =>
                        (s.SetupMode == mode || s.SetupMode == SetupMode.Any || ((mode is SetupMode.Update || mode is SetupMode.Restore) && s.SetupMode is SetupMode.UpdateAndRestore))
                        &&
                        (s.MachineRole == MachineRole.Any || s.MachineRole == this.machineRole))
                    .OrderBy(s => s.Number)
                    .ToArray();

                if (serviceSnapshot.ActiveStep != null)
                {
                    this.ActiveStep = this.Steps.SingleOrDefault(s => s.Number == serviceSnapshot.ActiveStep.Number);
                    this.logger.Debug($"Step #{this.ActiveStep?.Number} was restored as the active step.");
                }

                this.logger.Debug($"Setup mode is now '{this.setupModeService.Mode}'.");
                this.logger.Debug($"Steps loaded (total: {this.Steps.Count()}).");
                foreach (var step in this.Steps)
                {
                    this.logger.Debug($"Step #{step.Number} '{step.Title}' [{step.Execution.Status}]");
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error loading steps from file.");
                throw new InvalidOperationException(
                    $"Impossibile continuare, errore durante il caricamento degli steps da \"{Directory.GetCurrentDirectory()}\"");
            }
        }

        public void Run()
        {
            if (this.isStarted)
            {
                this.logger.Warn("Unable to start installation because installation is already started.");
                return;
            }

            if (!this.Steps.Any())
            {
                throw new InvalidOperationException("Unable to execute setup because no steps are available.");
            }

            if (this.Steps.Any(s => s.Execution.Status == StepStatus.RollbackFailed))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because rollback failed.");
            }

            if (this.Steps.Any(s => s.Execution.Status == StepStatus.InProgress))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was in progress.");
            }

            if (this.Steps.Any(s => s.Execution.Status == StepStatus.RollingBack))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was being rolled back.");
            }

            this.isStarted = true;
            this.CheckToSkipCurrentStartingStep();

            this.logger.Debug("All conditions met to start installation.");

            var runThread = new Thread(new ThreadStart(async () => await this.RunThreadAsync()));
            runThread.Name = "Installation Steps";
            runThread.Start();
        }

        public void SetConfiguration(Uri masUrl)
        {
            this.masUrl = masUrl;

            if (this.tsUrl != null)
            {
                var tsUri = new UriBuilder(masUrl);
                tsUri.Port = this.tsUrl.Port;
                this.tsUrl = tsUri.Uri;
            }
        }

        private void CheckToSkipCurrentStartingStep()
        {
            var stepInProgress = this.Steps.SingleOrDefault(s =>
                s.Execution.Status == StepStatus.InProgress
                &&
                s.SkipOnResume);

            if (stepInProgress != null)
            {
                this.logger.Debug($"Step #{stepInProgress.Number} - {stepInProgress.Title} was in progress: marking it as done.");

                stepInProgress.Execution.Status = StepStatus.Done;
                this.ActiveStep = stepInProgress;
                this.RaisePropertyChanged(nameof(this.ActiveStep));
            }
        }

        private void Dump()
        {
            this.logger.Debug("Taking snapshot of installation status ...");

            var objectString = JsonConvert.SerializeObject(new
            {
                this.Steps,
                this.IsRollbackInProgress,
                this.ActiveStep,
                SetupMode = this.setupModeService.Mode,
            }, SerializerSettings);

            const string OldFileName = "steps-snapshot.old.json";
            if (File.Exists("steps-snapshot.json"))
            {
                File.Move("steps-snapshot.json", OldFileName, true);
            }

            File.WriteAllText("steps-snapshot.json", objectString);

            if (File.Exists(OldFileName))
            {
                File.Delete(OldFileName);
            }

            this.logger.Debug("Snapshot taken.");
        }

        private void RaiseInstallationFinished(bool success)
        {
            this.Finished?.Invoke(this, new InstallationFinishedEventArgs(success));
        }

        private async Task RollbackStep(Step stepToRollback)
        {
            await stepToRollback.RollbackAsync();
            this.Dump();
            if (stepToRollback.Execution.Status is StepStatus.RollbackFailed)
            {
                throw new InvalidOperationException(
                    $"Unable to continue with setup because rollback of step {stepToRollback.Number} failed.");
            }
        }

        private async Task RunThreadAsync()
        {
            try
            {
                while (this.Steps.Any(s => s.Execution.Status != StepStatus.Done) || this.IsRollbackInProgress)
                // stop when all steps are done or when the first step was rolled back
                {
                    if (this.IsRollbackInProgress)
                    {
                        this.ActiveStep = this.Steps.TakeWhile(s => s.Execution.Status != StepStatus.RolledBack).LastOrDefault();

                        if (this.ActiveStep != null)
                        {
                            await this.RollbackStep(this.ActiveStep);
                        }
                        else
                        {
                            this.logger.Debug("Installation rollback completed.");
                            this.IsRollbackInProgress = false;
                            return;
                        }
                    }
                    else
                    {
                        this.ActiveStep = this.Steps.FirstOrDefault(s => s.Execution.Status == StepStatus.ToDo || s.Execution.Status == StepStatus.Failed || s.Execution.Status == StepStatus.RolledBack);
                        this.ActiveStep.Execution.Status = StepStatus.Start;
                        this.Dump();

                        this.RaisePropertyChanged(nameof(this.ActiveStep));

                        await this.ActiveStep.ApplyAsync();
                        this.Dump();

                        if (this.ActiveStep.Execution.Status is StepStatus.Failed || this.IsRollbackInProgress)
                        {
                            if (!this.ActiveStep.SkipRollback)
                            {
                                //this.IsRollbackInProgress = true;
                                await this.RollbackStep(this.ActiveStep);
                            }
                            this.logger.Debug("Installation interrupted.");
                            return;
                        }
                    }

                    await Task.Delay(500);
                }

                this.RaiseInstallationFinished(!this.IsRollbackInProgress);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);

                this.RaiseInstallationFinished(false);
                throw;
            }
            finally
            {
                this.isStarted = false;
            }
        }

        private void UpdateMachineRole()
        {
            this.MachineRole = this.masUrl?.Host?.Equals(this.ppcIpAddress?.ToString(), StringComparison.InvariantCultureIgnoreCase) == true
                ? MachineRole.Master
                : MachineRole.Slave;

            this.logger.Debug($"MachineRole is now '{this.MachineRole}'. masUrl {this.masUrl}, ppcIpAddress {this.ppcIpAddress}");
        }

        #endregion
    }
}
