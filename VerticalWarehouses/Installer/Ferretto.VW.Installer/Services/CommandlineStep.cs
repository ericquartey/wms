using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Services
{
    internal class CommandlineStep : IStep
    {
        #region Properties

        public string RollbackScript { get; set; }

        public string Script { get; set; }

        public StepStatus Status { get; set; }

        public string Title { get; set; }

        #endregion

        #region Methods

        public Task ApplyAsync()
        {
            if (this.Status is StepStatus.Done || this.Status is StepStatus.InProgress)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be executed because its status is {this.Status}.");
            }

            var process = System.Diagnostics.Process.Start("cmd.exe", $"/C {this.Script}");

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                this.Status = StepStatus.Failed;
            }

            return Task.CompletedTask;
        }

        public Task RollbackAsync()
        {
            if (this.Status is StepStatus.ToDo)
            {
                throw new InvalidOperationException($"Step '{this.Title}' cannot be rolled back because its status is {this.Status}.");
            }

            var process = System.Diagnostics.Process.Start("cmd.exe", $"/C {this.RollbackScript}");

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                this.Status = StepStatus.RollbackFailed;
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
