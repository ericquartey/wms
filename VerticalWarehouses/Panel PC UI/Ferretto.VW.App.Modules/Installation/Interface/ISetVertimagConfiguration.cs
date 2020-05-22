using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.Interface
{
    public interface ISetVertimagConfiguration
    {
        #region Properties

        FileInfo SelectedFileConfiguration { get; set; }

        InverterParametersData SelectedInverter { get; }

        VertimagConfiguration VertimagConfiguration { get; set; }

        #endregion

        #region Methods

        Task BackupVertimagConfigurationParameters();

        #endregion
    }
}
