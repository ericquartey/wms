using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed partial class SemiAutoMovementsViewModel
    {
        #region Fields

        private readonly IMachineCarouselWebService machineCarouselWebService;

        private bool hasCarousel;

        private bool isCarouselMoving;

        private DelegateCommand moveCarouselDownCommand;

        private ActionPolicy moveCarouselDownPolicy;

        private DelegateCommand moveCarouselUpCommand;

        private ActionPolicy moveCarouselUpPolicy;

        #endregion

        #region Properties

        public ICommand CarouselDownCommand =>
            this.moveCarouselDownCommand
            ??
            (this.moveCarouselDownCommand = new DelegateCommand(
            async () => await this.MoveCarouselDownAsync(),
            this.CanMoveCarouselDown));

        public ICommand CarouselUpCommand =>
            this.moveCarouselUpCommand
            ??
            (this.moveCarouselUpCommand = new DelegateCommand(
            async () => await this.MoveCarouselUpAsync(),
            this.CanMoveCarouselUp));

        public bool HasCarousel
        {
            get => this.hasCarousel;
            set => this.SetProperty(ref this.hasCarousel, value);
        }

        public bool IsCarouselMoving
        {
            get => this.isCarouselMoving;
            private set => this.SetProperty(ref this.isCarouselMoving, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        private bool CanMoveCarouselDown()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.moveCarouselDownPolicy?.IsAllowed == true;
        }

        private bool CanMoveCarouselUp()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse
                &&
                this.moveCarouselUpPolicy?.IsAllowed == true;
        }

        private async Task MoveCarouselDownAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAssistedAsync(VerticalMovementDirection.Down);
                this.IsCarouselMoving = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MoveCarouselUpAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAssistedAsync(VerticalMovementDirection.Up);
                this.IsCarouselMoving = true;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task TuneBayAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage(InstallationApp.ConfirmationOperation, "Movimenti semi-automatici", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    await this.machineCarouselWebService.FindZeroAsync();
                    this.IsTuningBay = true;
                }
            }
            catch (Exception ex)
            {
                this.IsTuningBay = false;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
