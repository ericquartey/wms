using System;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal class CommandlineStep : Step
    {
        #region Constructors

        public CommandlineStep(int number, string title, string rollbackScript, string script)
            : base(number, title)
        {
            if (rollbackScript is null)
            {
                throw new ArgumentNullException(nameof(rollbackScript));
            }

            if (script is null)
            {
                throw new ArgumentNullException(nameof(script));
            }

            this.RollbackScript = rollbackScript;
            this.Script = script;
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
            var success = this.TryRunCommandline(this.RollbackScript);

            return Task.FromResult(
                success
                    ? StepStatus.RolledBack
                    : StepStatus.RollbackFailed);
        }

        private bool TryRunCommandline(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Unable to run the commandline command.", nameof(command));
            }

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {command}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.Start();
                process.WaitForExit();
                this.Log = process.StandardOutput.ReadToEnd();

                return process.ExitCode >= 0;
            }
        }

        #endregion
    }
}
