using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Login.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Login.ViewModels
{
    internal sealed class LoginViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly IHealthProbeService healthProbeService;

        private readonly IBayManager bayManager;

        private readonly ISessionService sessionService;

        private SubscriptionToken subscriptionToken;

        private MachineIdentity machineIdentity;

        private DelegateCommand loginCommand;

        private HealthStatus serviceHealthStatus;

        private bool isWaitingForResponse;

        private int bayNumber;

        #endregion

        #region Constructors

        public LoginViewModel(
            IAuthenticationService authenticationService,
            IMachineErrorsService machineErrorsService,
            IHealthProbeService healthProbeService,
            ISessionService sessionService,
            IBayManager bayManager)
            : base(PresentationMode.Login)
        {
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.ServiceHealthStatus = this.healthProbeService.HealthStatus;

#if DEBUG
            this.UserLogin = new UserLogin
            {
                UserName = "installer",
                Password = "password",
            };
#else
            this.UserLogin = new UserLogin();
#endif
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand LoginCommand =>
            this.loginCommand
            ??
            (this.loginCommand = new DelegateCommand(
                async () => await this.LoginAsync(),
                this.CanExecuteLogin));

        public UserLogin UserLogin { get; }

        public int BayNumber
        {
            get => this.bayNumber;
            set
            {
                this.SetProperty(ref this.bayNumber, value);
            }
        }

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

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

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

        public void OnHealthStatusChanged(HealthStatusChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this.ServiceHealthStatus = e.HealthStatus;

            if (this.ServiceHealthStatus == HealthStatus.Degraded
                ||
                this.ServiceHealthStatus == HealthStatus.Healthy)
            {
                this.ClearNotifications();
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.subscriptionToken = this.healthProbeService.HealthStatusChanged
                    .Subscribe(
                        this.OnHealthStatusChanged,
                        ThreadOption.UIThread,
                        false);

                this.IsWaitingForResponse = true;
                var bay = await this.bayManager.GetBayAsync();
                if (!(bay is null))
                {
                    this.BayNumber = (int)bay.Number;
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

            this.machineErrorsService.AutoNavigateOnError = false;

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
            }
        }

        private bool CanExecuteLogin()
        {
            return
                this.machineIdentity != null

                // &&
                // string.IsNullOrEmpty(this.UserLogin.Error)
                &&
                !this.IsWaitingForResponse
                &&
                (this.ServiceHealthStatus == HealthStatus.Healthy || this.ServiceHealthStatus == HealthStatus.Degraded);
        }

        private void RaiseCanExecuteChanged()
        {
            this.loginCommand?.RaiseCanExecuteChanged();
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
                   this.UserLogin.Password);

                if (claims != null)
                {
                    this.sessionService.SetUserAccessLevel(claims.AccessLevel);

                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Menu),
                        Utils.Modules.Menu.MAINMENU,
                        data: this.Data,
                        trackCurrentView: true);

                    this.machineErrorsService.AutoNavigateOnError = true;
                }
                else
                {
                    this.ShowNotification(Resources.Errors.UserLogin_InvalidCredentials, Services.Models.NotificationSeverity.Error);
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

        private void NavigateToInstallerMainView()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.INSTALLATORMENU,
                data: null,
                trackCurrentView: true);
        }

        private void NavigateToOperatorMainView()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.OPERATORMENU,
                data: null,
                trackCurrentView: true);
        }
    }
}
