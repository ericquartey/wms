using System;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.Installer.Core.Models.Steps;

#nullable enable

namespace Ferretto.VW.Installer.Core
{
    public abstract class Step : BindableBase
    {
        #region Fields

        private const string EmbeddedInstallationFilePath = "EmbeddedInstallationFilePath";

        #endregion

        #region Constructors

        public Step(int number, string title, string description, MachineRole machineRole, SetupMode setupMode, bool skipOnResume, bool skipRollback)
        {
            this.Title = title;
            this.Number = number;
            this.MachineRole = machineRole;
            this.SetupMode = setupMode;
            this.Description = description;
            this.SkipOnResume = skipOnResume;
            this.SkipRollback = skipRollback;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public StepExecution Execution { get; set; } = new StepExecution();

        public MachineRole MachineRole { get; }

        public int Number { get; }

        public SetupMode SetupMode { get; }

        /// <summary>
        /// When true, it specifies that, if the installation was resumed from a snapshot and the current step is active,
        /// then it should not be executed, but considered as completed instead.
        /// </summary>
        public bool SkipOnResume { get; }

        /// <summary>
        /// When true, it specifies that, in case of error on this step, the rollback procedure shall not be started.
        /// </summary>
        public bool SkipRollback { get; }

        public string Title { get; }

        #endregion

        #region Methods

        public static string InterpolateVariables(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var regex = new Regex(@"\$\((?<var_name>[^\$)]+)\)");

            var match = regex.Match(value);

            var interpolatedValue = value;
            while (match.Success)
            {
                var varName = match.Groups["var_name"].Value;
                string varValue;
                if (varName == EmbeddedInstallationFilePath)
                {
                    varValue = Process.GetCurrentProcess().MainModule.FileName;
                }
                else
                {
                    varValue = ConfigurationManager.AppSettings.Get(varName);
                    if (varValue is null)
                    {
                        throw new Exception($"La chiave di configurazione '{varName}' non esiste.");
                    }
                }

                interpolatedValue = interpolatedValue.Replace(match.Value, varValue, StringComparison.Ordinal);
                match = match.NextMatch();
            }

            return interpolatedValue;
        }

        public async Task ApplyAsync()
        {
            if (this.Execution.Status is StepStatus.Done || this.Execution.Status is StepStatus.InProgress)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be executed because its status is {this.Execution.Status}.");
            }

            this.Execution.LogInformation($"Avvio dello step '{this.Title}'.");

            this.Execution.StartTime = DateTime.Now;
            var timer = new Timer(this.OnTimerTick, null, 0, 500);

            this.Execution.Status = StepStatus.InProgress;

            try
            {
                this.Execution.Status = await this.OnApplyAsync();
                timer.Dispose();
                this.Execution.EndTime = DateTime.Now;
                this.Execution.LogInformation($"Step completato con stato '{this.Execution.Status}'.");
            }
            catch (Exception ex)
            {
                this.Execution.Status = StepStatus.Failed;
                this.Execution.LogError($"Step fallito inaspettatamente: {ex.Message}");
                timer.Dispose();
            }
        }

        public async Task RollbackAsync()
        {
            if (this.Execution.Status != StepStatus.Failed && this.Execution.Status != StepStatus.Done)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be rolled back because its status is {this.Execution.Status}.");
            }

            this.Execution.LogInformation("Avvio rollback.");

            this.Execution.Status = StepStatus.RollingBack;

            try
            {
                this.Execution.Status = await this.OnRollbackAsync();
                this.Execution.LogInformation($"Rollback of step completed with status {this.Execution.Status}.");
            }
            catch
            {
                this.Execution.LogInformation("Rollback dello step fallito inaspettatamente.");

                this.Execution.Status = StepStatus.RollbackFailed;
            }
        }

        public override string ToString()
        {
            return $"{this.Number}: {this.Title} ({this.Execution.Status})";
        }

        protected abstract Task<StepStatus> OnApplyAsync();

        protected abstract Task<StepStatus> OnRollbackAsync();

        private void OnTimerTick(object? state)
        {
            this.Execution.Duration = DateTime.Now - this.Execution.StartTime;
        }

        #endregion
    }
}
