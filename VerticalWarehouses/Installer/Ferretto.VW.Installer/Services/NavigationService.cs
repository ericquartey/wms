using System;
using System.Threading.Tasks;
using Ferretto.VW.Installer.Core;
using Ferretto.VW.Installer.ViewModels;
using NLog;

#nullable enable

namespace Ferretto.VW.Installer.Services
{
    public sealed class NavigationService : BindableBase, INavigationService
    {
        #region Fields

        private static readonly NavigationService instance = new NavigationService(NotificationService.GetInstance());

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly INotificationService notificationService;

        private IViewModel? activeViewModel;

        private IViewModel? previousViewModel;

        #endregion

        #region Constructors

        private NavigationService(INotificationService notificationService)
        {
            if (notificationService is null)
            {
                throw new ArgumentNullException(nameof(notificationService));
            }

            this.notificationService = notificationService;
        }

        #endregion

        #region Properties

        public IViewModel? ActiveViewModel
        {
            get => this.activeViewModel;
            private set => this.SetProperty(ref this.activeViewModel, value);
        }

        #endregion

        #region Methods

        public void NavigateBack()
        {
            this.logger.Debug($"Navigating back to '{this.previousViewModel?.GetType()?.Name}'.");

            this.ActiveViewModel = this.previousViewModel;
        }

        public async Task NavigateToAsync(IViewModel viewModel)
        {
            if (viewModel is null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            this.logger.Debug($"Navigating to '{viewModel.GetType().Name}'.");

            this.previousViewModel = this.ActiveViewModel;
            if (this.previousViewModel != null)
            {
                try
                {
                    await this.previousViewModel.OnDisappearAsync();
                }
                catch (Exception ex)
                {
                    this.notificationService.SetErrorMessage(
                        $"An error occurred while leaving the current page: {ex.Message}");
                }
            }

            try
            { 
                await viewModel.OnAppearAsync();
                this.ActiveViewModel = viewModel;
            }
            catch(Exception ex)
            {
                this.notificationService.SetErrorMessage(
                    $"An error occurred while trying to navigate to the next page: {ex.Message}");
            }
        }

        internal static INavigationService GetInstance()
        {
            return instance;
        }

        #endregion
    }
}
