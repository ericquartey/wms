using System;
using System.Linq;
using System.Management.Automation;
using System.Threading;

namespace Ferretto.VW.Installer.Core
{
    internal class PowershellStep : ShellStep
    {
        #region Fields

        private int? progressPercentage;

        #endregion

        #region Constructors

        public PowershellStep(int number, string title, string description, string rollbackScript, string script)
            : base(number, title, description, rollbackScript, script)
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

        protected override bool TryRunCommandline(string command)
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
                    var result = shell.BeginInvoke();

                    while (!result.IsCompleted)
                    {
                        var p = shell.Streams.Progress.LastOrDefault();
                        this.ProgressPercentage = p?.PercentComplete;
                        Thread.Sleep(300);
                    }

                    // result.Select(r => r.Properties["Message"]?.ToString()).ToList().ForEach(l => this.LogInformation(l));

                    //shell.Streams.Information.Select(i => i.MessageData.ToString()).ToList().ForEach(l => this.LogInformation(l));

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

        #endregion
    }
}
