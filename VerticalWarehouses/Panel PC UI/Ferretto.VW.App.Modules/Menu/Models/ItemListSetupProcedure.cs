using System.Windows.Input;
using Ferretto.VW.App.Menu.ViewModels;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Menu.Models
{
    public class ItemListSetupProcedure : BindableBase
    {
        #region Fields

        private bool bypassable;

        private ICommand command;

        private InstallationStatus status;

        private string text;

        #endregion

        #region Properties

        public bool Bypassable
        {
            get => this.bypassable;
            set => this.SetProperty(ref this.bypassable, value);
        }

        public ICommand Command
        {
            get => this.command;
            set => this.SetProperty(ref this.command, value);
        }

        public InstallationStatus Status
        {
            get => this.status;
            set => this.SetProperty(ref this.status, value);
        }

        public string Text
        {
            get => this.text;
            set => this.SetProperty(ref this.text, value);
        }

        #endregion
    }
}
