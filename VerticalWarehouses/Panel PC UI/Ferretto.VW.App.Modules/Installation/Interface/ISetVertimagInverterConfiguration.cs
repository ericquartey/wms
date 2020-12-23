using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.Interface
{
    public interface ISetVertimagInverterConfiguration
    {
        #region Properties

        FileInfo SelectedFileConfiguration { get; set; }

        Inverter SelectedInverter { get; }

        IEnumerable<Inverter> VertimagInverterConfiguration { get; set; }

        #endregion

        #region Methods

        void BackupVertimagInverterConfigurationParameters();

        #endregion
    }
}
