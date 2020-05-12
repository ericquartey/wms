using System.Collections.Generic;
using Ferretto.VW.App.Scaffolding.Design;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Installation.Interface
{
    public interface ISetVertimagConfiguration
    {
        #region Properties

        string SelectedFileConfigurationName { get; set; }

        VertimagConfiguration VertimagConfiguration { get; set; }

        #endregion
    }
}
