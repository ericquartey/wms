using System;
using NLog;

namespace Ferretto.VW.Installer.Core
{
    internal sealed class SetupModeService : ISetupModeService
    {
        #region Fields

        private const string RestoreCommandlineArg = "--restore";

        private const string UpdateCommandlineArg = "--update";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private SetupMode mode = SetupMode.Install;

        #endregion

        #region Constructors

        public SetupModeService()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public SetupMode Mode
        {
            get => this.mode;
            set
            {
                if (this.mode != value)
                {
                    this.logger.Info($"Setup mode changed from '{this.mode}' to '{value}'.");
                    this.mode = value;
                }
            }
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.Equals(UpdateCommandlineArg, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Mode = SetupMode.Update;
                    break;
                }
                else if (arg.Equals(RestoreCommandlineArg, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.Mode = SetupMode.Restore;
                    break;
                }
            }

            this.logger.Info($"Setup mode is '{this.Mode}'.");
        }

        #endregion
    }
}
