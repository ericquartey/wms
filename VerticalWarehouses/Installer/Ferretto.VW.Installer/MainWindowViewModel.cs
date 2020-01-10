using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal sealed class MainWindowViewModel
    {
        #region Fields

        private readonly InstallationService installationService = new InstallationService();

        #endregion

        #region Constructors

        public MainWindowViewModel()
        {
        }

        #endregion

        #region Properties

        public IEnumerable<IStep> Steps => this.installationService.Steps;

        #endregion
    }
}
