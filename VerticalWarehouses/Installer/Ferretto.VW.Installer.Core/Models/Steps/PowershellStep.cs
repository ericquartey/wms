using System;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal class PowershellStep : ShellStep
    {
        #region Fields

        private int? progressPercentage;

        #endregion

        #region Constructors

        public PowershellStep(
            int number,
            string title,
            string description,
            string rollbackScript,
            string script,
            MachineRole machineRole,
            SetupMode setupMode,
            bool skipOnResume,
            bool skipRollback)
            : base(number, title, description, rollbackScript, script, machineRole, setupMode, skipOnResume, skipRollback)
        {
        }

        #endregion

        #region Properties

        public int? ProgressPercentage
        {
            get => this.progressPercentage;
            set => this.SetProperty(ref this.progressPercentage, value);
        }

        #endregion

        #region Methods

        protected override async Task<bool> TryRunCommandlineAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Unable to run the powershell command.", nameof(command));
            }

            command = InterpolateVariables(command);

            this.Execution.LogInformation("");
            this.Execution.LogInformation($"ps> {command}");

            try
            {
                using var shell = PowerShell.Create();
                shell.AddScript(command);

                using var output = new PSDataCollection<PSObject>();
                output.DataAdded += (object snd, DataAddedEventArgs evt) => this.OnDataAdded(snd, evt, shell);
                var result = await shell.InvokeAsync<PSObject, PSObject>(null, output);
                if (shell.HadErrors)
                {
                    this.Execution.LogError(shell.Streams.Error.LastOrDefault()?.Exception?.Message);
                    this.Execution.LogError("Errors encountered while running the script.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.Execution.LogError(ex.Message);

                return false;
            }

            return true;
        }

        private void OnDataAdded(object sender, DataAddedEventArgs evt, PowerShell shell)
        {
            var col = (PSDataCollection<PSObject>)sender;
            var rsl = col.ReadAll();

            foreach (var row in rsl)
            {
                var rowString = row.ToString();
                this.Execution.LogInformation($"> {rowString.Replace("\0", "")}");
            }

            var p = shell.Streams.Progress.LastOrDefault();
            this.ProgressPercentage = p?.PercentComplete;
        }

        #endregion
    }
}
