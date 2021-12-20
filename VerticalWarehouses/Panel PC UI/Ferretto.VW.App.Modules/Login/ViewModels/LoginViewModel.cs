using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.TokenReader;
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

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly ISessionService sessionService;

        private readonly ICardReaderService cardReaderService;

        private readonly ITokenReaderService tokenReaderService;

        private readonly ILocalizationService localizationService;

        private DelegateCommand loginCommand;

        private MachineIdentity machineIdentity;

        private HealthStatus serviceHealthStatus;

        private SubscriptionToken subscriptionToken;

        private readonly EventHandler<RegexMatchEventArgs> cardReaderTokenAcquiredEventHandler;

        private readonly EventHandler<TokenStatusChangedEventArgs> tokenReaderTokenStatusChangedEventHandler;

        private ObservableCollection<string> users;

        private System.Collections.Generic.IEnumerable<User> wmsUsers;

        private readonly IMachineUsersWebService usersService;

        private IEnumerable<UserParameters> userList;

        #endregion

        #region Constructors

        public LoginViewModel(
            IMachineUsersWebService usersService,
            IAuthenticationService authenticationService,
            IMachineErrorsService machineErrorsService,
            IHealthProbeService healthProbeService,
            ISessionService sessionService,
            ICardReaderService cardReaderService,
            ITokenReaderService tokenReaderService,
            IBayManager bayManager,
            IBarcodeReaderService barcodeReaderService,
            IMachineBaysWebService machineBaysWebService,
            IMachineIdentityWebService machineIdentityWebService,
            ILocalizationService localizationService)
            : base(PresentationMode.Login)
        {
            this.usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.cardReaderService = cardReaderService ?? throw new ArgumentNullException(nameof(cardReaderService));
            this.tokenReaderService = tokenReaderService ?? throw new ArgumentNullException(nameof(tokenReaderService));
            this.ServiceHealthStatus = this.healthProbeService.HealthMasStatus;
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            this.Users = new ObservableCollection<string>(this.BaseUser);
#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "vertimag2020",
            };
#else
            this.UserLogin = new UserLogin();
#endif

            this.UserLogin.PropertyChanged += this.UserLogin_PropertyChanged;
            this.MachineService.PropertyChanged += this.MachineService_PropertyChanged;

            this.cardReaderTokenAcquiredEventHandler = new EventHandler<RegexMatchEventArgs>(
                async (sender, e) => await this.OnCardReaderTokenAcquired(sender, e));

            this.tokenReaderTokenStatusChangedEventHandler = new EventHandler<TokenStatusChangedEventArgs>(
                async (sender, e) => await this.OnTokenReaderTokenAcquired(sender, e));
        }

        private async Task OnCardReaderTokenAcquired(object sender, RegexMatchEventArgs e)
        {
            try
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.LoggingInUsingCard"), Services.Models.NotificationSeverity.Info);

                var claims = await this.authenticationService.LogInAsync(e.Token);

                await this.NavigateToMainMenuAsync(claims);

                this.Logger.Info($"Login user {claims.Name} level {claims.AccessLevel}");
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Unable to authenticate user with card.");
                this.ShowNotification(Resources.Localized.Get("LoadLogin.UnableToAuthenticateWithTheCard"), Services.Models.NotificationSeverity.Warning);
            }
        }

        private async Task OnTokenReaderTokenAcquired(object sender, TokenStatusChangedEventArgs e)
        {
            if (!e.IsInserted)
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.TokenRemoved"), Services.Models.NotificationSeverity.Info);
                return;
            }
            else if (e.SerialNumber is null)
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.TokenInserted"), Services.Models.NotificationSeverity.Info);
                return;
            }

            if (!this.IsWmsHealthy)
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.UnableToAuthenticateWithTheTokenBecauseWmsIsNotReachable"), Services.Models.NotificationSeverity.Warning);
                return;
            }

            try
            {
                this.ShowNotification(Resources.Localized.Get("LoadLogin.AuthenticatingUser"), Services.Models.NotificationSeverity.Info);

                var claims = await this.authenticationService.LogInAsync(e.SerialNumber);

                await this.NavigateToMainMenuAsync(claims);

                this.Logger.Info($"Login user {claims.Name} level {claims.AccessLevel}");
            }
            catch (Exception ex)
            {
                this.Logger.Error(ex, $"Unable to authenticate user with token.");
                this.ShowNotification(Resources.Localized.Get("LoadLogin.UnableToAuthenticateWithTheToken"), Services.Models.NotificationSeverity.Error);
            }
        }

        #endregion

        private readonly List<string> BaseUser = new List<string>() { "operator", "installer", "service", "admin" };

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

        public ObservableCollection<string> Users
        {
            get => this.users;
            set
            {
                if (this.SetProperty(ref this.users, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public System.Collections.Generic.IEnumerable<User> WmsUsers
        {
            get => this.wmsUsers;
            set
            {
                if (this.SetProperty(ref this.wmsUsers, value))
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

                    this.Logger.Info($"Login user {claims.Name} level {claims.AccessLevel}");
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
            this.cardReaderService.TokenAcquired -= this.cardReaderTokenAcquiredEventHandler;

            this.tokenReaderService.TokenStatusChanged -= this.tokenReaderTokenStatusChangedEventHandler;

            this.UserLogin.IsValidationEnabled = true;
            this.UserLogin.Password = null;

            if (this.subscriptionToken != null)
            {
                this.healthProbeService.HealthStatusChanged.Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        private void SetBaseUser()
        {
            if (!this.Users.ToList().SequenceEqual(this.BaseUser))
            {
                this.Users.Clear();
                this.Users.AddRange(this.BaseUser);
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsKeyboardButtonVisible = await this.machineIdentityWebService.GetTouchHelperEnableAsync();

            this.sessionService.IsLogged = false;

            this.SetBaseUser();

            this.ClearNotifications();

            this.subscriptionToken = this.healthProbeService.HealthStatusChanged
                .Subscribe(
                    async (e) => await this.OnHealthStatusChangedLocalAsync(e),
                    ThreadOption.UIThread,
                    false);

            this.machineErrorsService.AutoNavigateOnError = false;

            this.MachineIdentity = this.sessionService.MachineIdentity;
            if (this.MachineIdentity is null)
            {
                try
                {
                    this.MachineIdentity = await this.machineIdentityWebService.GetAsync();
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
            }

            this.IsVisible = true;
            this.IsEnabled = true;

            this.tokenReaderService.TokenStatusChanged += this.tokenReaderTokenStatusChangedEventHandler;
            this.cardReaderService.TokenAcquired += this.cardReaderTokenAcquiredEventHandler;

            await this.barcodeReaderService.StartAsync();
            await this.cardReaderService.StartAsync();
            await this.tokenReaderService.StartAsync();

            if (this.authenticationService.IsAutoLogoutServiceUser)
            {
                this.authenticationService.IsAutoLogoutServiceUser = false;
                this.ShowNotification(Resources.Localized.Get("LoadLogin.AutoLogoutServiceUser"));
            }

            this.userList = await this.usersService.GetAllUserWithCultureAsync();

            this.SelectedUserChanged();

            await this.SetUsers();
        }

        private void SelectedUserChanged()
        {
            switch (this.UserLogin?.UserName?.ToLower())
            {
                case "installer":
                    Localized.Instance.CurrentKeyboardCulture = new System.Globalization.CultureInfo(this.userList.ToList().Find(x => x.Name == this.UserLogin.UserName).Language);
                    break;

                case "service":
                    Localized.Instance.CurrentKeyboardCulture = new System.Globalization.CultureInfo(this.userList.ToList().Find(x => x.Name == this.UserLogin.UserName).Language);
                    break;

                case "admin":
                    Localized.Instance.CurrentKeyboardCulture = new System.Globalization.CultureInfo(this.userList.ToList().Find(x => x.Name == this.UserLogin.UserName).Language);
                    break;

                default:
                    Localized.Instance.CurrentKeyboardCulture = new System.Globalization.CultureInfo(this.userList.ToList().Find(x => x.Name.ToLower() == "operator").Language);
                    break;
            }
        }

        private async Task SetUsers()
        {
            try
            {
                this.WmsUsers = await this.usersService.GetAllUsersAsync();

                var wmsUsersName = this.WmsUsers.Select(s => s.Login).ToList();

                var isOperatorEnabled = await this.usersService.GetOperatorEnabledWithWMSAsync();

                if (isOperatorEnabled)
                {
                    var userToAdd = wmsUsersName.Except(this.BaseUser);
                    this.Users.AddRange(userToAdd);
                }
                else
                {
                    var newBaseUser = this.BaseUser.Where(s => s != "operator").ToList();

                    var userToAdd = wmsUsersName.Except(newBaseUser);

                    this.Users.Remove("operator");
                    this.Users.AddRange(userToAdd);
                }
            }
            catch (Exception)
            {
                //this.ShowNotification("WMS NO USERS", Services.Models.NotificationSeverity.Error);

                this.SetBaseUser();
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Users));
            }
        }

        private async Task OnHealthStatusChangedLocalAsync(HealthStatusChangedEventArgs e)
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

            if (e.HealthWmsStatus == HealthStatus.Healthy)
            {
                await this.barcodeReaderService.StartAsync();
                await this.SetUsers();
            }
            else
            {
                await this.barcodeReaderService.StopAsync();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.loginCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecuteLogin()
        {
            if (this.UserLogin.UserName == "operator")
            {
                return
                this.machineIdentity != null
                &&
                !this.IsWaitingForResponse
                &&
                (this.ServiceHealthStatus == HealthStatus.Healthy || this.ServiceHealthStatus == HealthStatus.Degraded);
            }
            else
            {
                return
                //this.machineIdentity != null
                //&&
                !this.IsWaitingForResponse;
            }
        }

        private async Task LoginAsync()
        {
            this.ClearNotifications();

            this.UserLogin.IsValidationEnabled = true;
            //if (!string.IsNullOrEmpty(this.UserLogin.Error))
            //{
            //    this.ShowNotification(this.UserLogin.Error, Services.Models.NotificationSeverity.Error);
            //    return;
            //}

            this.IsWaitingForResponse = true;

            try
            {
                if (this.BaseUser.Contains(this.UserLogin.UserName))
                {
                    if (!string.IsNullOrEmpty(this.UserLogin.Error))
                    {
                        this.ShowNotification(this.UserLogin.Error, Services.Models.NotificationSeverity.Error);
                        return;
                    }

                    var claims = await this.authenticationService.LogInAsync(
                       this.UserLogin.UserName,
                       this.UserLogin.Password,
                       this.UserLogin.SupportToken);

                    await this.NavigateToMainMenuAsync(claims);

                    this.Logger.Info($"Login user {claims.Name} level {claims.AccessLevel}");
                }
                else
                {
                    //var claimWms = await this.usersService.AuthenticateWithResourceOwnerPasswordAsync(
                    //    this.UserLogin.UserName,
                    //    this.UserLogin.Password);

                    var claims = await this.authenticationService.LogInAsync(
                       this.UserLogin.UserName,
                       this.UserLogin.Password,
                       this.UserLogin.SupportToken,
                       UserAccessLevel.Operator);

                    await this.NavigateToMainMenuAsync(claims);

                    this.Logger.Info($"Login user {claims.Name} level {claims.AccessLevel}");
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (!this.sessionService.IsLogged)
                {
                    this.ShowNotification(Resources.Localized.Get("LoadLogin.InvalidCredentials"), Services.Models.NotificationSeverity.Error);
                }
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

        private async Task NavigateToMainMenuAsync(UserClaims claims)
        {
            if (claims != null)
            {
                this.sessionService.SetUserAccessLevel(claims.AccessLevel);

                await this.machineBaysWebService.ActivateAsync();

                Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Menu),
                            Utils.Modules.Menu.MAIN_MENU,
                            data: this.Data,
                            trackCurrentView: true);

                        this.machineErrorsService.AutoNavigateOnError = true;

                        if (this.UserLogin.UserName == "service")
                        {
                            this.localizationService.ActivateCulture(UserAccessLevel.Support);
                        }
                        else
                        {
                            this.localizationService.ActivateCulture(claims.AccessLevel);
                        }

                        this.sessionService.IsLogged = true;
                    });
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

            this.loginCommand?.RaiseCanExecuteChanged();

            this.SelectedUserChanged();
        }

        #endregion
    }
}
