using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.LowSpeedMovements
{
    internal class LSMTGateEngineViewModel : BindableBase
    {
        ICommand action;

        public ICommand Action => this.action ?? (this.action = new DelegateCommand(() => Debug.Print("Ciao")));
    }
}
