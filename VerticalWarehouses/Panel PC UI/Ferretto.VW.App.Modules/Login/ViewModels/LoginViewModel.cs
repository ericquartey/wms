using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
    internal sealed class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBayManager bayManager;

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly ISessionService sessionService;

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
            IBayManager bayManager,
            IMachineBaysWebService machineBaysWebService)
            : base(PresentationMode.Login)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.ServiceHealthStatus = this.healthProbeService.HealthMasStatus;
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "vertimag2020",
            };
#endif

            this.UserLogin.PropertyChanged += this.UserLogin_PropertyChanged;
            this.MachineService.PropertyChanged += this.MachineService_PropertyChanged;
        }

        #endregion

        #region Properties

        public int BayNumber
        {
            get => (int)this.MachineService?.BayNumber;
        }

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

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

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

            // await base.OnAppearedAsync();
            this.IsVisible = true;
            this.IsEnabled = true;
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
                this.ShowNotification(Resources.LoadLogin.ConnectionLost, Services.Models.NotificationSeverity.Error);
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
                }
                else
                {
                    this.ShowNotification(Resources.LoadLogin.InvalidCredentials, Services.Models.NotificationSeverity.Error);
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void MachineService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
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
