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

        private double? bayChainHorizontalPosition;

        private DelegateCommand carouselDownCommand;

        private DelegateCommand carouselUpCommand;

        private bool hasCarousel;

        private bool isCarouselMoving;

        #endregion

        #region Properties

        public double? BayChainHorizontalPosition
        {
            get => this.bayChainHorizontalPosition;
            private set => this.SetProperty(ref this.bayChainHorizontalPosition, value);
        }

        public ICommand CarouselDownCommand =>
            this.carouselDownCommand
            ??
            (this.carouselDownCommand = new DelegateCommand(
                async () => await this.CarouselDownAsync(),
                this.CanExecuteCarouselDownCommand));

        public ICommand CarouselUpCommand =>
            this.carouselUpCommand
            ??
            (this.carouselUpCommand = new DelegateCommand(
                async () => await this.CarouselUpAsync(),
                this.CanExecuteCarouselUpCommand));

        public bool HasCarousel
        {
            get => this.hasCarousel;
            set => this.SetProperty(ref this.hasCarousel, value);
        }

        public bool IsCarouselMoving
        {
            get => this.isCarouselMoving;
            private set
            {
                if (this.SetProperty(ref this.isCarouselMoving, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsCarouselMoving));
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        private bool CanExecuteCarouselDownCommand()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;
            // &&
            // (this.Sensors.LUPresentMiddleBottomBay1); //IoStatus.LoadingUnitExistenceInBay
        }

        private bool CanExecuteCarouselUpCommand()
        {
            return
                !this.KeyboardOpened
                &&
                !this.IsMoving
                &&
                !this.IsWaitingForResponse;
            // &&
            // IoStatus.LoadingUnitInLowerBay;
        }

        private async Task CarouselDownAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAsync(HorizontalMovementDirection.Backwards, null);
                this.IsCarouselMoving = true;
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

        private async Task CarouselUpAsync()
        {
            this.IsWaitingForResponse = true;

            try
            {
                await this.machineCarouselWebService.MoveAsync(HorizontalMovementDirection.Forwards, null);
                this.IsCarouselMoving = true;
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
