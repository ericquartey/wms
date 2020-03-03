using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal class PowershellStep : ShellStep
    {
        #region Fields

        private int? progressPercentage;

        #endregion

        #region Constructors

        public PowershellStep(int number, string title, string description, string rollbackScript, string script, string log, MachineRole machineRole, SetupMode setupMode, bool skipOnResume)
            : base(number, title, description, rollbackScript, script, log, machineRole, setupMode, skipOnResume)
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

            this.LogInformation("");
            this.LogInformation($"ps> {command}");

            try
            {
                using (var shell = PowerShell.Create())
                {
                    shell.AddScript(command);

                    var output = new PSDataCollection<PSObject>();
                    output.DataAdded += (object snd, DataAddedEventArgs evt) => this.output_DataAdded(snd, evt, shell);                    
                    var result = await shell.InvokeAsync<PSObject, PSObject>(null, output);
                    if (shell.HadErrors)
                    {
                        this.LogError(shell.Streams.Error.LastOrDefault()?.Exception?.Message);
                        this.LogError("Errors encountered while running the script." );
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);

                return false;
            }

            return true;
        }


        void output_DataAdded(object snd, DataAddedEventArgs evt, PowerShell shell)
        {
            var col = (PSDataCollection<PSObject>)snd;
            var rsl = col.ReadAll();

            foreach (PSObject r in rsl)
            {
                this.LogInformation(r.ToString());                
            }
            
            var p = shell.Streams.Progress.LastOrDefault();
            this.ProgressPercentage = p?.PercentComplete;
        }

        #endregion
    }
}
