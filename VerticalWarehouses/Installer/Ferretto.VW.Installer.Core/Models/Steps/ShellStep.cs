using System;
using System.IO;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal abstract class ShellStep : Step
    {
        #region Constructors

        public ShellStep(
            int number,
            string title,
            string description,
            string rollbackScript,
            string script,
            MachineRole machineRole,
            SetupMode setupMode,
            bool skipOnResume,
            bool skipRollback)
            : base(number, title, description, machineRole, setupMode, skipOnResume, skipRollback)
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

        protected override async Task<StepStatus> OnApplyAsync()
        {
            var script = LoadScriptFromFile(this.Script);
            var success = await this.TryRunCommandlineAsync(script);

            return
                success
                    ? StepStatus.Done
                    : StepStatus.Failed;
        }

        protected override async Task<StepStatus> OnRollbackAsync()
        {
            if (string.IsNullOrWhiteSpace(this.RollbackScript))
            {
                this.Execution.LogInformation("Nulla da annullare in questo step.");
                return StepStatus.RolledBack;
            }

            var script = LoadScriptFromFile(this.RollbackScript);

            var success = await this.TryRunCommandlineAsync(script);

            return
                success
                    ? StepStatus.RolledBack
                    : StepStatus.RollbackFailed;
        }

        protected abstract Task<bool> TryRunCommandlineAsync(string command);

        private static string LoadScriptFromFile(string script)
        {
            if (File.Exists(script))
            {
                return File.ReadAllText(script);
            }

            return script;
        }

        #endregion
    }
}
