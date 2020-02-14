using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using NLog;

namespace Ferretto.VW.Installer.Core
{
    public sealed class InstallationService : BindableBase
    {
        #region Fields

        private const string INSTALLARG = "-install";

        private const string UPDATEARG = "-update";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Newtonsoft.Json.Formatting.Indented,
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Step activeStep;

        private bool isInstall;

        private bool isRollbackInProgress;

        private bool isUpdate;

        private string masVersion;

        private OperationMode operationMode;

        private string panelPcVersion;

        private string softwareVersion;

        #endregion

        #region Events

        public event EventHandler<InstallationFinishedEventArgs> Finished;

        #endregion

        #region Properties

        public Step ActiveStep
        {
            get => this.activeStep;
            private set => this.SetProperty(ref this.activeStep, value);
        }

        public bool IsInstall => this.isInstall;

        public bool IsRollbackInProgress
        {
            get => this.isRollbackInProgress;
            private set => this.SetProperty(ref this.isRollbackInProgress, value);
        }

        public bool IsUpdate => this.isUpdate;

        public string MasVersion
        {
            get => this.masVersion;
            private set => this.SetProperty(ref this.masVersion, value);
        }

        public OperationMode OperationMode
        {
            get => this.operationMode;
            private set => this.SetProperty(ref this.operationMode, value);
        }

        public string PanelPcVersion
        {
            get => this.panelPcVersion;
            private set => this.SetProperty(ref this.panelPcVersion, value);
        }

        public string SoftwareVersion
        {
            get => this.softwareVersion;
            private set => this.SetProperty(ref this.softwareVersion, value);
        }

        public IEnumerable<Step> Steps { get; private set; }

        #endregion

        #region Methods

        public static InstallationService LoadAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("message", nameof(fileName));
            }

            var stepsJsonFile = File.ReadAllText(fileName);

            var serviceAnon = new { Steps = Array.Empty<Step>() };
            serviceAnon = JsonConvert.DeserializeAnonymousType(stepsJsonFile, serviceAnon, SerializerSettings);

            return new InstallationService()
            {
                Steps = serviceAnon.Steps.OrderBy(s => s.Number).ToArray()
            };
        }

        public void Abort()
        {
            this.IsRollbackInProgress = true;
        }

        public bool CanStart()
        {
            this.SetArgsStartup();
            if (!(this.isInstall || this.isUpdate))
            {
                if (this.Steps.FirstOrDefault(s => s.StartTime != null) is null)
                {
                    this.logger.Debug("Nothing to do, update/installation in progress.");
                    return false;
                }
            }

            return true;
        }

        public Step GetNextStepToExecute()
        {
            return this.Steps.FirstOrDefault(s => s.Status == StepStatus.ToDo);
        }

        public void LoadMasVersion()
        {
            this.MasVersion = this.GetVersion("../panelpc_app/properties/app.manifest");
        }

        public void LoadPanelPcVersion()
        {
            this.PanelPcVersion = this.GetVersion("../automation_service/properties/app.manifest");
        }

        public async Task RunAsync()
        {
            if (!this.Steps.Any())
            {
                throw new InvalidOperationException("Unable to execute setup because no steps are available.");
            }

            if (this.Steps.Any(s => s.Status == StepStatus.RollbackFailed))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because rollback failed.");
            }

            if (this.Steps.Any(s => s.Status == StepStatus.InProgress))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was in progress.");
            }

            if (this.Steps.Any(s => s.Status == StepStatus.RollingBack))
            {
                this.IsRollbackInProgress = true;
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was being rolled back.");
            }

            this.CheckToSkipCurrentStartingStep();

            try
            {
                while (this.Steps.Any(s => s.Status != StepStatus.Done) || this.IsRollbackInProgress)
                // stop when all steps are done or when the first step was rolled back
                {
                    if (this.IsRollbackInProgress)
                    {
                        this.ActiveStep = this.Steps.TakeWhile(s => s.Status != StepStatus.RolledBack).LastOrDefault();

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
                        this.ActiveStep = this.Steps.FirstOrDefault(s => s.Status == StepStatus.ToDo);
                        this.ActiveStep.Status = StepStatus.Start;
                        this.Dump();
                        this.RaisePropertyChanged(nameof(this.ActiveStep));
                        await this.ActiveStep.ApplyAsync();
                        this.Dump();
                        if (this.ActiveStep.Status is StepStatus.Failed || this.IsRollbackInProgress)
                        {
                            if (!this.ActiveStep.SkipRollback)
                            {
                                break;
                                //this.IsRollbackInProgress = true;
                                //await this.RollbackStep(this.ActiveStep);
                            }
                        }
                    }
                }
            }
            catch
            {
                this.RaiseInstallationFinished(false);
            }

            this.RaiseInstallationFinished(!this.IsRollbackInProgress);
        }

        public void Start()
        {
            this.LoadMasVersion();
            this.LoadPanelPcVersion();
            this.LoadMasVersion();

            if (this.isInstall)
            {
                this.OperationMode = OperationMode.Imstall;
            }
            else if (this.isUpdate)
            {
                this.OperationMode = OperationMode.Update;
            }
        }

        private void CheckToSkipCurrentStartingStep()
        {
            var stepInProgress = this.Steps.SingleOrDefault(s => s.Status == StepStatus.Start
                                                                 &&
                                                                 s.SkipOnResume);
            if (!(stepInProgress is null))
            {
                stepInProgress.Status = StepStatus.Done;
                this.ActiveStep = stepInProgress;
                this.RaisePropertyChanged(nameof(this.ActiveStep));
            }
        }

        private void Dump()
        {
            var objectString = JsonConvert.SerializeObject(this, SerializerSettings);

            System.IO.File.WriteAllText("steps-snapshot.json", objectString);
        }

        private string GetVersion(string fullPath)
        {
            try
            {
                using (var reader = new StreamReader(fullPath))
                {
                    using (var xmlReader = XmlReader.Create(reader))
                    {
                        while (xmlReader.Read())
                        {
                            if (xmlReader.NodeType == XmlNodeType.Element
                                &&
                                xmlReader.Name == "assemblyIdentity")
                            {
                                if (xmlReader.HasAttributes)
                                {
                                    return xmlReader.GetAttribute("version");
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private void LoadSoftwareVersion()
        {
            this.SoftwareVersion = this.GetVersion("./properties/app.manifest");
        }

        private void RaiseInstallationFinished(bool success)
        {
            this.Finished?.Invoke(this, new InstallationFinishedEventArgs(success));
        }

        private async Task RollbackStep(Step stepToRollback)
        {
            await stepToRollback.RollbackAsync();
            this.Dump();
            if (stepToRollback.Status is StepStatus.RollbackFailed)
            {
                throw new InvalidOperationException(
                    $"Unable to continue with setup because rollback of step {stepToRollback.Number} failed.");
            }
        }

        private void SetArgsStartup()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.ToLower(CultureInfo.InvariantCulture) == UPDATEARG)
                {
                    this.isUpdate = true;
                }
                else if (arg.ToLower(CultureInfo.InvariantCulture) == INSTALLARG)
                {
                    this.isInstall = true;
                }
            }
        }

        #endregion
    }
}
