using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal class PowershellStep : ShellStep
    {
        #region Constructors

        public PowershellStep(int number, string title, string description, string rollbackScript, string script)
            : base(number, title, description, rollbackScript, script)
        {
        }

        #endregion

        #region Methods

        protected override bool TryRunCommandline(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Unable to run the powershell command.", nameof(command));
            }

            command = this.InterpolateVariables(command);

            this.LogInformation($"ps>{command}");

            try
            {
                using (var shell = PowerShell.Create())
                {
                    shell.AddScript(command);
                    var result = shell.Invoke();

                    var logs = result.Select(r => r.Properties["Message"]?.ToString()).ToList();
                    logs.ForEach(l => this.LogInformation(l));

                    if (shell.HadErrors)
                    {
                        this.LogError("Errors encountered while running the script.");
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

        #endregion
    }
}
