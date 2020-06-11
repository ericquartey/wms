using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Ferretto.VW.Installer.Core
{
    public sealed class InstallationService : BindableBase
    {
        #region Fields

        private const string INSTALLARG = "--install";

        private const string RESTOREARG = "--restore";

        private const string UPDATEARG = "--update";

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Newtonsoft.Json.Formatting.Indented
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string stepsFileName;

        private Step activeStep;

        private bool isRollbackInProgress;

        private bool isStartedFromSnapShot;

        private MachineRole machineRole;

        private MAS.DataModels.VertimagConfiguration masConfiguration;

        private IPAddress masIpAddress;

        private string masVersion;

        private OperationStage operationMode;

        private string panelPcVersion;

        private IPAddress ppcIpAddress;

        private SetupMode setupMode;

        private string softwareVersion;

        #endregion

        #region Constructors

        public InstallationService(string fileName)
        {
            this.stepsFileName = fileName;
        }

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

        public bool IsRollbackInProgress
        {
            get => this.isRollbackInProgress;
            private set => this.SetProperty(ref this.isRollbackInProgress, value);
        }

        public MachineRole MachineRole
        {
            get => this.machineRole;
            private set => this.SetProperty(ref this.machineRole, value);
        }

        [JsonIgnore]
        public MAS.DataModels.VertimagConfiguration MasConfiguration => this.masConfiguration;

        [JsonIgnore]
        public IPAddress MasIpAddress => this.masIpAddress;

        public string MasVersion
        {
            get => this.masVersion;
            private set => this.SetProperty(ref this.masVersion, value);
        }

        public OperationStage OperationStage
        {
            get => this.operationMode;
            private set => this.SetProperty(ref this.operationMode, value);
        }

        public string PanelPcVersion
        {
            get => this.panelPcVersion;
            private set => this.SetProperty(ref this.panelPcVersion, value);
        }

        public SetupMode SetupMode
        {
            get => this.setupMode;
            private set => this.SetProperty(ref this.setupMode, value);
        }

        public string SoftwareVersion
        {
            get => this.softwareVersion;
            private set => this.SetProperty(ref this.softwareVersion, value);
        }

        public IEnumerable<Step> Steps { get; private set; }

        #endregion

        #region Methods

        public static InstallationService GetInstance(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("message", nameof(fileName));
            }

            return new InstallationService(fileName);
        }

        public void Abort()
        {
            this.IsRollbackInProgress = true;
        }

        public bool CanStart()
        {
            if (this.SetupMode == SetupMode.Any)
            {
                if (this.Steps.FirstOrDefault(s => s.StartTime != null) is null)
                {
                    this.logger.Debug("Nothing to do, update/installation in progress.");
                    return false;
                }
            }

            return true;
        }

        public void GetInfoFromSnapShot()
        {
            try
            {
                this.isStartedFromSnapShot = true;
                var stepsJsonFile = File.ReadAllText(this.stepsFileName);
                var parsedObject = JObject.Parse(stepsJsonFile);
                this.IsRollbackInProgress = (bool)parsedObject[nameof(this.IsRollbackInProgress)].ToObject<bool>();
                this.SetupMode = (SetupMode)parsedObject[nameof(this.SetupMode)].ToObject<SetupMode>();
            }
            catch (Exception ex)
            {
                var msg = $" Unable to read/assign data from Snapshot file \"{this.stepsFileName}\"";
                this.logger.Error(ex, msg);
                throw new InvalidOperationException(msg);
            }
        }

        public void GetInstallerParameters()
        {
            try
            {
                this.masIpAddress = null;
                this.ppcIpAddress = null;

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var masIpAddress = config.AppSettings.Settings["Install:Parameter:MasIpaddress"].Value;
                if (!string.IsNullOrEmpty(masIpAddress)
                    &&
                    IPAddress.TryParse(masIpAddress.ToString(), out var masIpAddressFound))
                {
                    this.masIpAddress = masIpAddressFound;
                }

                var ppcIpAddress = config.AppSettings.Settings["Install:Parameter:PpcIpaddress"].Value;
                if (!string.IsNullOrEmpty(ppcIpAddress)
                  &&
                  IPAddress.TryParse(ppcIpAddress.ToString(), out var ppcIpAddressFound))
                {
                    this.ppcIpAddress = ppcIpAddressFound;
                }
            }
            catch (Exception)
            {
                this.masIpAddress = null;
                this.ppcIpAddress = null;
            }
        }

        public Step GetNextStepToExecute()
        {
            return this.Steps.FirstOrDefault(s => s.Status == StepStatus.ToDo);
        }

        public void LoadMasVersion()
        {
            this.MasVersion = this.GetVersionFromManifest("../panelpc_app/properties/app.manifest");
        }

        public void LoadPanelPcVersion()
        {
            this.PanelPcVersion = this.GetVersionFromManifest("../automation_service/properties/app.manifest");
        }

        public void LoadSteps()
        {
            this.logger.Debug($"Loading execution steps from '{Path.Combine(Directory.GetCurrentDirectory(), this.stepsFileName)}' ...");
            try
            {
                var stepsJsonFile = File.ReadAllText(this.stepsFileName);
                var serviceAnon = new { Steps = Array.Empty<Step>() };
                serviceAnon = JsonConvert.DeserializeAnonymousType(stepsJsonFile, serviceAnon, SerializerSettings);
                this.Steps = serviceAnon.Steps.Where(s => (s.SetupMode == this.setupMode || s.SetupMode == SetupMode.Any || ((this.setupMode == SetupMode.Update || this.setupMode == SetupMode.Restore) && s.SetupMode == SetupMode.UpdateAndRestore))
                                                          &&
                                                          (s.MachineRole == this.machineRole || s.MachineRole == MachineRole.Any))
                                                          .OrderBy(s => s.Number).ToArray();

                this.logger.Debug($"Steps loaded (total: {this.Steps.Count()}).");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, $"Error loading steps from file.");
                throw new InvalidOperationException(
                                  $"Impossibile continuare, errore durante il caricmento degli steps da \"{Directory.GetCurrentDirectory()}\"");
            }
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
                                this.IsRollbackInProgress = true;
                                await this.RollbackStep(this.ActiveStep);
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

        public void SaveVertimagConfiguration(string configurationFilePath, string fileContents)
        {
            try
            {
                File.WriteAllText(configurationFilePath, fileContents);
            }
            catch (Exception ex)
            {
                var msg = $" Error wrting configuration file \"{configurationFilePath}\"";
                this.logger.Error(ex, msg);
                throw new InvalidOperationException(msg);
            }
        }

        public void SetArgsStartup()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.ToLower(CultureInfo.InvariantCulture) == UPDATEARG)
                {
                    this.logger.Info("Application launched as 'update' mode.");
                    this.SetupMode = SetupMode.Update;
                }
                else if (arg.ToLower(CultureInfo.InvariantCulture) == RESTOREARG)
                {
                    this.logger.Info("Application launched as 'restore' mode.");
                    this.SetupMode = SetupMode.Restore;
                }
                else
                {
                    this.logger.Info("Application launched as 'install' mode.");
                    this.SetupMode = SetupMode.Install;
                }
            }
        }

        public void SetConfiguration(IPAddress masIpAddress, MAS.DataModels.VertimagConfiguration masConfiguration)
        {
            this.masIpAddress = masIpAddress;
            this.masConfiguration = masConfiguration;
        }

        public void SetStage(OperationStage stage)
        {
            this.OperationStage = stage;
        }

        public void Start()
        {
            this.LoadSoftwareVersion();
            this.LoadPanelPcVersion();
            this.LoadMasVersion();

            if (this.isStartedFromSnapShot)
            {
                this.OperationStage = OperationStage.Update;
                return;
            }

            if (this.setupMode == SetupMode.Install)
            {
                this.OperationStage = OperationStage.RoleSelection;
            }
            else if (this.setupMode == SetupMode.Update
                     ||
                     this.setupMode == SetupMode.Restore)
            {
                this.OperationStage = OperationStage.Update;
            }
        }

        public void UpdateMachineRole()
        {
            this.MachineRole = (this.masIpAddress.Equals(this.ppcIpAddress)) ? MachineRole.Master : MachineRole.Slave;
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

        private string GetVersionFromManifest(string fullPath)
        {
            this.logger.Debug($"Retrieving software version from manifest '{fullPath}' ...");
            try
            {
                using var reader = new StreamReader(fullPath);
                using var xmlReader = XmlReader.Create(reader);
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element
                        &&
                        xmlReader.Name == "assemblyIdentity")
                    {
                        if (xmlReader.HasAttributes)
                        {
                            var version = xmlReader.GetAttribute("version");

                            this.logger.Debug($"Found software version '{version}'.");

                            return version;
                        }
                    }
                }
            }
            catch
            {
                this.logger.Error("Could not retrieve software version.");
            }

            return null;
        }

        private void LoadSoftwareVersion()
        {
            this.SoftwareVersion = this.GetVersionFromManifest("./properties/app.manifest");
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

        #endregion
    }
}
