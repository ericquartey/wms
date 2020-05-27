﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    [Warning(WarningsArea.Login)]
    internal sealed class LoginViewModel : BaseMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly ISessionService sessionService;

        private readonly ICardReaderService cardReaderService;

        private readonly ILocalizationService localizationService;

        private DelegateCommand loginCommand;

        private MachineIdentity machineIdentity;

        private HealthStatus serviceHealthStatus;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            IMachineErrorsService machineErrorsService,
            IHealthProbeService healthProbeService,
            ISessionService sessionService,
            ICardReaderService cardReaderService,
            IBayManager bayManager,
            IBarcodeReaderService barcodeReaderService,
            IMachineBaysWebService machineBaysWebService,
            ILocalizationService localizationService)
            : base(PresentationMode.Login)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.cardReaderService = cardReaderService;
            this.ServiceHealthStatus = this.healthProbeService.HealthMasStatus;
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "vertimag2020",
            };
#else
            this.UserLogin = new UserLogin { };
#endif

            this.UserLogin.PropertyChanged += this.UserLogin_PropertyChanged;
            this.MachineService.PropertyChanged += this.MachineService_PropertyChanged;
            this.cardReaderService.TokenAcquired += async (sender, e) => await this.OnCardReaderTokenAcquired(sender, e);
        }

        private async Task OnCardReaderTokenAcquired(object sender, string e)
        {
            try
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.LoggingInUsingCard"), Services.Models.NotificationSeverity.Info);

                var claims = await this.authenticationService.LogInAsync(e);

                await this.NavigateToMainMenuAsync(claims);
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Unable to authenticate user with card.");
                this.ShowNotification(Resources.Localized.Get("LoadLogin.UnableToAuthenticateWithTheCard"), Services.Models.NotificationSeverity.Warning);
            }
        }

        #endregion

        public string ActiveContextName => "Login";

        #region Properties

        public int BayNumber => (int)this.MachineService?.BayNumber;

        public override EnableMask EnableMask => EnableMask.Any;

        public override bool KeepAlive => false;

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(
                async () => await this.LoginAsync(),
                this.CanExecuteLogin));

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set
            {
                if (this.SetProperty(ref this.machineIdentity, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public HealthStatus ServiceHealthStatus
        {
            get => this.serviceHealthStatus;
            set
            {
                if (this.SetProperty(ref this.serviceHealthStatus, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public UserLogin UserLogin { get; }

        protected override bool IsDataRefreshSyncronous => true;

        public async Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return;
            }

            if (e.UserAction is UserAction.LoginUser)
            {
                try
                {
                    this.ClearNotifications();
                    var bearerToken = e.GetBearerToken();
                    var claims = await this.authenticationService.LogInAsync(bearerToken);

                    await this.NavigateToMainMenuAsync(claims);
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, $"Unable to authenticate user with barcode");
                    this.ShowNotification(Resources.Localized.Get("LoadLogin.UnableToAuthenticateWithTheBarcode"), Services.Models.NotificationSeverity.Warning);
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.cardReaderService.StopAsync();

            this.UserLogin.IsValidationEnabled = true;
            this.UserLogin.Password = null;

            if (this.subscriptionToken != null)
            {
                this.healthProbeService.HealthStatusChanged.Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.ClearNotifications();

            this.subscriptionToken = this.healthProbeService.HealthStatusChanged
                .Subscribe(
                    this.OnHealthStatusChanged,
                    ThreadOption.UIThread,
                    false);

            this.machineErrorsService.AutoNavigateOnError = false;

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
                this.sessionService.MachineIdentity = machineIdentity;
            }
            else
            {
                this.MachineIdentity = this.sessionService.MachineIdentity;
            }

            this.IsVisible = true;
            this.IsEnabled = true;

            await this.barcodeReaderService.StartAsync();
            await this.cardReaderService.StartAsync();
        }

        public void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.ServiceHealthStatus = e.HealthMasStatus;

            if (this.ServiceHealthStatus == HealthStatus.Degraded
                ||
                this.ServiceHealthStatus == HealthStatus.Healthy)
            {
                this.ClearNotifications();
            }
            else
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.ConnectionLost"), Services.Models.NotificationSeverity.Error);
                this.RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.loginCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteLogin()
        {
            return
                this.machineIdentity != null
                &&
                !this.IsWaitingForResponse
                &&
                (this.ServiceHealthStatus == HealthStatus.Healthy || this.ServiceHealthStatus == HealthStatus.Degraded);
        }

        private async Task LoginAsync()
        {
            this.ClearNotifications();

            this.UserLogin.IsValidationEnabled = true;
            if (!string.IsNullOrEmpty(this.UserLogin.Error))
            {
                this.ShowNotification(this.UserLogin.Error, Services.Models.NotificationSeverity.Error);
                return;
            }

            this.IsWaitingForResponse = true;

            try
            {
                var claims = await this.authenticationService.LogInAsync(
                   this.UserLogin.UserName,
                   this.UserLogin.Password,
                   this.UserLogin.SupportToken);

                switch (this.UserLogin.UserName)
                {
                    case "admin":
                        ScaffolderUserAccesLevel.User = UserAccessLevel.Admin;
                        break;

                    case "service":
                        ScaffolderUserAccesLevel.User = UserAccessLevel.Support;
                        break;

                    case "installer":
                        ScaffolderUserAccesLevel.User = UserAccessLevel.Installer;
                        break;

                    case "operator":
                        ScaffolderUserAccesLevel.User = UserAccessLevel.Operator;
                        break;

                    default:
                        ScaffolderUserAccesLevel.User = UserAccessLevel.NoAccess;
                        break;
                }

                await this.NavigateToMainMenuAsync(claims);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(Ferretto.VW.App.Resources.Localized.Get("LoadLogin.InvalidCredentials"), Services.Models.NotificationSeverity.Error);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task NavigateToMainMenuAsync(UserClaims claims)
        {
            if (claims != null)
            {
                this.sessionService.SetUserAccessLevel(claims.AccessLevel);

                await this.machineBaysWebService.ActivateAsync();

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Menu),
                    Utils.Modules.Menu.MAIN_MENU,
                    data: this.Data,
                    trackCurrentView: true);

                this.machineErrorsService.AutoNavigateOnError = true;

                this.localizationService.ActivateCulture(claims.AccessLevel);
            }
            else
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.InvalidCredentials"), Services.Models.NotificationSeverity.Error);
            }
        }

        private void MachineService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.PropertyName == nameof(this.MachineService.BayNumber))
                {
                    try
                    {
                        this.RaisePropertyChanged(nameof(this.BayNumber));
                    }
                    catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                    {
                    }
                }
            }));
        }

        private void UserLogin_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                if (e.PropertyName == nameof(this.UserLogin.UserName) && this.UserLogin.IsSupport)
                {
                    if (this.ServiceHealthStatus == HealthStatus.Healthy || this.ServiceHealthStatus == HealthStatus.Degraded)
                    {
                        try
                        {
                            this.UserLogin.Password = string.Empty;
                            this.UserLogin.SupportToken = await this.authenticationService.GetToken();
                        }
                        catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                        {
                        }
                    }
                }
            }));
        }

        #endregion
    }
}
