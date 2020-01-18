using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ferretto.VW.Installer
{
    internal sealed class InstallationService : BindableBase
    {
        #region Fields

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
        };

        private Step activeStep;

        #endregion

        #region Properties

        public Step ActiveStep
        {
            get => this.activeStep;
            private set => this.SetProperty(ref this.activeStep, value);
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

            var stepsJsonFile = System.IO.File.ReadAllText(fileName);

            var serviceAnon = new { Steps = Array.Empty<Step>() };
            serviceAnon = JsonConvert.DeserializeAnonymousType(stepsJsonFile, serviceAnon, SerializerSettings);

            return new InstallationService()
            {
                Steps = serviceAnon.Steps.OrderBy(s => s.Number).ToArray()
            };
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
                throw new InvalidOperationException("Unable to continue with setup because rollback failed.");
            }

            if (this.Steps.Any(s => s.Status == StepStatus.InProgress))
            {
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was in progress.");
            }

            if (this.Steps.Any(s => s.Status == StepStatus.RollingBack))
            {
                throw new InvalidOperationException("Unable to continue with setup because execution was interrupted while one step was being rolled back.");
            }

            try
            {
                while (this.Steps.Any(s => s.Status != StepStatus.Done) || this.Steps.FirstOrDefault(s => s.MustRollback)?.Status is StepStatus.RolledBack)
                // stop when all steps are done or when the first step was rolled back
                {
                    var stepToRollback = this.Steps.Any(s => s.Status == StepStatus.RolledBack)
                        ? this.Steps.TakeWhile(s => s.Status != StepStatus.RolledBack).LastOrDefault()
                        : null;

                    if (stepToRollback is null)
                    {
                        this.ActiveStep = this.Steps.FirstOrDefault(s => s.Status == StepStatus.ToDo);
                        this.Dump();
                        this.RaisePropertyChanged(nameof(this.ActiveStep));

                        await this.ActiveStep.ApplyAsync();
                        this.Dump();
                        if (this.ActiveStep.Status is StepStatus.Failed && this.ActiveStep.MustRollback)
                        {
                            await this.RollbackStep(this.ActiveStep);
                        }
                    }
                    else
                    {
                        await this.RollbackStep(stepToRollback);
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
