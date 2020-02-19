using System.Linq;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Ferretto.VW.App.Menu.ViewModels;

namespace Ferretto.VW.App.Modules.Menu.Models
{
    public class ItemListSetupProcedure
    {
        public string Text { get; set; }

        public InstallationStatus Status { get; set; }

        public bool Bypassable { get; set; }

        public DelegateCommand Command { get; set; }
    }
}
