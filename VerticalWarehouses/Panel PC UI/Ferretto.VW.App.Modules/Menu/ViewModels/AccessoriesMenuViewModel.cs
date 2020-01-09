using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class AccessoriesMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private DelegateCommand laserCommand;

        #endregion

        #region Constructors

        public AccessoriesMenuViewModel()
            : base()
        {
        }

        #endregion

        #region Properties

        public ICommand LaserCommand =>
            this.laserCommand
            ??
            (this.laserCommand = new DelegateCommand(
                () => this.ExecuteCommand(),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.laserCommand?.RaiseCanExecuteChanged();
        }

        private void ExecuteCommand()
        {
        }

        #endregion
    }
}
