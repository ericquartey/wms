using System;
using System.IO;
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

            this.Script = LoadScript(script);
            this.RollbackScript = LoadScript(rollbackScript);
        }

        #endregion

        #region Properties

        public string RollbackScript { get; }

        public string Script { get; }

        #endregion

        #region Methods

        protected override async Task<StepStatus> OnApplyAsync()
        {
            var success = await this.TryRunCommandlineAsync(this.Script);

            return
                success
                    ? StepStatus.Done
                    : StepStatus.Failed;
        }

        protected override async Task<StepStatus> OnRollbackAsync()
        {
            if (string.IsNullOrWhiteSpace(this.RollbackScript))
            {
                this.LogInformation("Nulla da annullare in questo step.");
                return StepStatus.RolledBack;
            }

            var success = await this.TryRunCommandlineAsync(this.RollbackScript);

            return
                success
                    ? StepStatus.RolledBack
                    : StepStatus.RollbackFailed;
        }

        protected abstract Task<bool> TryRunCommandlineAsync(string command);

        private static string LoadScript(string script)
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
