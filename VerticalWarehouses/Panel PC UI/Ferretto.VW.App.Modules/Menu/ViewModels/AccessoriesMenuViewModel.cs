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
    internal sealed class AccessoriesMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isWaitingForResponse;

        private DelegateCommand laserCommand;

        #endregion

        #region Constructors

        public AccessoriesMenuViewModel()
            : base(PresentationMode.Menu)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public ICommand LaserCommand =>
            this.laserCommand
            ??
            (this.laserCommand = new DelegateCommand(
                () => this.ExecuteCommand(),
                this.CanExecuteCommand));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsWaitingForResponse = false;
        }

        private bool CanExecuteCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void ExecuteCommand()
        {
        }

        private void RaiseCanExecuteChanged()
        {
            this.laserCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
