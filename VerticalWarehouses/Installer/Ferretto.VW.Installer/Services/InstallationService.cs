using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using NLog;

namespace Ferretto.VW.Installer
{
    internal sealed class InstallationService : BindableBase
    {
        #region Fields

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Newtonsoft.Json.Formatting.Indented,
        };

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private Step activeStep;

        private bool isRollbackInProgress;

        private string softwareVersion;

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

        public Step GetNextStepToExecute()
        {
            return this.Steps.FirstOrDefault(s => s.Status == StepStatus.ToDo);
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

            this.LoadSoftwareVersion();

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
                // do nothing
            }
        }

        private void Dump()
        {
            var objectString = JsonConvert.SerializeObject(this, SerializerSettings);

            System.IO.File.WriteAllText("steps-snapshot.json", objectString);
        }

        private void LoadSoftwareVersion()
        {
            var packagePath = ConfigurationManager.AppSettings.Get("Install:Package:Path");

            try
            {
                using (var package = System.IO.Compression.ZipFile.OpenRead(packagePath))
                {
                    var manifestEntry = package.Entries.FirstOrDefault(e => e.FullName.Contains("app.manifest"));

                    using (var stream = manifestEntry.Open())
                    {
                        using (var xmlReader = XmlReader.Create(stream))
                        {
                            while (xmlReader.Read())
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element
                                    &&
                                    xmlReader.Name == "assemblyIdentity")
                                {
                                    if (xmlReader.HasAttributes)
                                    {
                                        this.SoftwareVersion = xmlReader.GetAttribute("version");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
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
