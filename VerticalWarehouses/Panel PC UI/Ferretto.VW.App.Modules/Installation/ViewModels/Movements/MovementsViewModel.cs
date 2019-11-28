using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class MovementsViewModel : BaseMainViewModel
    {
        #region Fields

        private DelegateCommand goToMovementsGuidedCommand;

        private DelegateCommand goToMovementsManualCommand;

        private bool isMovementsGuided = true;

        private bool isWaitingForResponse;

        private string title;

        #endregion

        #region Constructors

        public MovementsViewModel()
            : base(PresentationMode.Installer)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.MachinePoweredOn;

        public ICommand GoToMovementsGuidedCommand =>
            this.goToMovementsGuidedCommand
            ??
            (this.goToMovementsGuidedCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(true),
                this.CanGoToMovementsExecuteCommand));

        public ICommand GoToMovementsManualCommand =>
            this.goToMovementsManualCommand
            ??
            (this.goToMovementsManualCommand = new DelegateCommand(
                () => this.GoToMovementsExecuteCommand(false),
                this.CanGoToMovementsExecuteCommand));

        public bool IsMovementsGuided => this.isMovementsGuided;

        public bool IsMovementsManual => !this.isMovementsGuided;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value, this.RaiseCanExecuteChanged);
        }

        public string Title
        {
            get => this.title;
            set => this.SetProperty(ref this.title, value, this.RaiseCanExecuteChanged);
        }

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

            try
            {
                this.GoToMovementsExecuteCommand(this.isMovementsGuided);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        protected override async Task OnMachineModeChangedAsync(MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);
            this.RaiseCanExecuteChanged();
        }

        private bool CanGoToMovementsExecuteCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void GoToMovementsExecuteCommand(bool isGuided)
        {
            this.isMovementsGuided = isGuided;
            if (isGuided)
            {
                this.Title = "Movimenti Guidati";
            }
            else
            {
                this.Title = "Movimenti Manuali";
            }
        }

        private void RaiseCanExecuteChanged()
        {
            this.goToMovementsGuidedCommand?.RaiseCanExecuteChanged();
            this.goToMovementsManualCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsMovementsGuided));
            this.RaisePropertyChanged(nameof(this.IsMovementsManual));
        }

        #endregion
    }
}
