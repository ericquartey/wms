using System;
using System.Diagnostics;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class ServiceBuilder
    {
        #region Constructors

        public ServiceBuilder(string serviceName)
        {
            this.ServiceName = serviceName;
        }

        #endregion

        #region Properties

        public string ServiceName { get; }

        #endregion

        #region Methods

        public int Register(string userName, string password)
        {
            try
            {
                var binPath = typeof(Program).Assembly.Location;
                var serviceStopCommand = $"sc stop \"{this.ServiceName}\"";
                var serviceDeleteCommand = $"sc delete \"{this.ServiceName}\"";

                var serviceCreateCommand = $"sc create \"{this.ServiceName}\" binPath= \"{binPath} {Program.ServiceConsoleArgument}\" obj= \"{userName}\" password= \"{password}\"";
                var serviceConfigCommand = $"sc config \"{this.ServiceName}\" start=auto";
                var serviceStartCommand = $"sc start \"{this.ServiceName}\"";

                RunShellCommand(serviceStopCommand, true);
                RunShellCommand(serviceDeleteCommand, true);
                RunShellCommand("timeout /T 10", true);
                RunShellCommand(serviceCreateCommand);
                RunShellCommand(serviceConfigCommand);
                RunShellCommand(serviceStartCommand);

                return 0;
            }
            catch
            {
                return -1;
            }
        }

        private static void RunShellCommand(string command, bool ignoreFailures = false)
        {
            Console.WriteLine($"> {command}");
            var createServiceProcess = Process.Start("cmd.exe", $"/C {command}");
            createServiceProcess.WaitForExit();
            if (createServiceProcess.ExitCode < 0 && !ignoreFailures)
            {
                throw new Exception();
            }
        }

        #endregion
    }
}
