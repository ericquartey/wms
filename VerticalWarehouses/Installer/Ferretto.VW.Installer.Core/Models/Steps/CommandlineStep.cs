using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Core
{
    internal class CommandlineStep : ShellStep
    {
        #region Constructors

        public CommandlineStep(int number, string title, string description, string rollbackScript, string script, string log, MachineRole machineRole, SetupMode setupMode, bool skipOnResume)
            : base(number, title, description, rollbackScript, script, log, machineRole, setupMode, skipOnResume)
        {
        }

        #endregion

        #region Methods

        protected override Task<bool> TryRunCommandlineAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentException("Unable to run the commandline command.", nameof(command));
            }

            command = InterpolateVariables(command);

            using (var process = new System.Diagnostics.Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/C {command}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                this.LogInformation($"cmd> {command}");

                process.Start();
                var thread = new Thread(new ParameterizedThreadStart(this.ReadStandardOutput));
                thread.Start(process.StandardOutput);
                process.WaitForExit();
                thread.Join();

                return Task.FromResult(process.ExitCode >= 0);
            }
        }

        private void ReadStandardOutput(object obj)
        {
            var inputStream = obj as StreamReader;

            if (inputStream is null)
            {
                return;
            }

            try
            {
                while (!inputStream.EndOfStream)
                {
                    this.LogChar((char)inputStream.Read());
                }
            }
            catch
            {
                // do nothing
            }
        }

        #endregion
    }
}
