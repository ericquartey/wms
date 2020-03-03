using System;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal abstract class ShellStep : Step
    {
        #region Constructors

        public ShellStep(int number, string title, string description, string rollbackScript, string script, string log, MachineRole machineRole, SetupMode setupMode, bool skipOnResume)
            : base(number, title, description, log, machineRole, setupMode, skipOnResume)
        {
            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            this.Script = script;
            this.RollbackScript = rollbackScript;
        }

        #endregion

        #region Properties

        public string RollbackScript { get; }

        public string Script { get; }

        #endregion

        #region Methods

        protected override Task<StepStatus> OnApplyAsync()
        {
            var success = this.TryRunCommandline(this.Script);

            return Task.FromResult(
                success
                    ? StepStatus.Done
                    : StepStatus.Failed);
        }

        protected override Task<StepStatus> OnRollbackAsync()
        {
            if (string.IsNullOrWhiteSpace(this.RollbackScript))
            {
                this.LogInformation("Nulla da annullare in questo step.");
                return Task.FromResult(StepStatus.RolledBack);
            }

            var success = this.TryRunCommandline(this.RollbackScript);

            return Task.FromResult(
                success
                    ? StepStatus.RolledBack
                    : StepStatus.RollbackFailed);
        }

        protected abstract bool TryRunCommandline(string command);

        #endregion
    }
}
