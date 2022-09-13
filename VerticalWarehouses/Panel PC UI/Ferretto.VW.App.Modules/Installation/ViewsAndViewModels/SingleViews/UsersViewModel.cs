using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class UsersViewModel : BaseMainViewModel
    {
        #region Fields

        private const int MinimumPasswordLength = 2;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private readonly IMachineUsersWebService usersService;

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private DelegateCommand changeGuestPasswordCommand;

        private DelegateCommand changeInstallerPasswordCommand;

        private DelegateCommand changeMovementPasswordCommand;

        private DelegateCommand changeOperatorPasswordCommand;

        private bool guestActive;

        private DelegateCommand guestCommand;

        private string guestNewPassword;

        private string guestNewPasswordConfirm;

        private bool installerActive;

        private DelegateCommand installerCommand;

        private string installerNewPassword;

        private string installerNewPasswordConfirm;

        private bool isEnabledEditing;

        private bool isGuestEnabled;

        private bool isMovementEnabled;

        private bool isOperatorEnabledWithWMS;

        private bool isWmsEnabled;

        private bool movementActive;

        private DelegateCommand movementCommand;

        private string movementNewPassword;

        private string movementNewPasswordConfirm;

        private bool operatorActive;

        private DelegateCommand operatorCommand;

        private string operatorNewPassword;

        private string operatorNewPasswordConfirm;

        private DelegateCommand saveIsGuestEnabledCommand;

        private DelegateCommand saveIsMovementEnabledCommand;

        private DelegateCommand saveIsOperatorEnabledWithWMSCommand;

        #endregion

        #region Constructors

        public UsersViewModel(IMachineUsersWebService usersService,
            IMachineWmsStatusWebService wmsStatusWebService,
            ISessionService sessionService,
            IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Installer)
        {
            this.usersService = usersService ?? throw new ArgumentNullException(nameof(usersService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.wmsStatusWebService = wmsStatusWebService ?? throw new ArgumentNullException(nameof(wmsStatusWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
        }

        #endregion

        #region Properties

        public ICommand ChangeGuestPasswordCommand =>
           this.changeGuestPasswordCommand
           ??
           (this.changeGuestPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword("guest"),
               this.CanExecuteGuestCommand));

        public ICommand ChangeInstallerPasswordCommand =>
                   this.changeInstallerPasswordCommand
           ??
           (this.changeInstallerPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword("installer"),
               this.CanExecuteInstallerCommand));

        public ICommand ChangeMovementPasswordCommand =>
           this.changeMovementPasswordCommand
           ??
           (this.changeMovementPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword("movement"),
               this.CanExecuteMovementCommand));

        public ICommand ChangeOperatorPasswordCommand =>
                   this.changeOperatorPasswordCommand
           ??
           (this.changeOperatorPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword("operator"),
               this.CanExecuteOperatorCommand));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool GuestActive
        {
            get => this.guestActive;
            set
            {
                this.SetProperty(ref this.guestActive, value, this.RaiseCanExecuteChanged);

                if (value)
                {
                    this.InstallerActive = false;
                    this.MovementActive = false;
                    this.OperatorActive = false;
                }
            }
        }

        public ICommand GuestCommand =>
           this.guestCommand
           ??
           (this.guestCommand = new DelegateCommand(
               () => this.GuestActive = true));

        public string GuestNewPassword
        {
            get => this.guestNewPassword;
            set => this.SetProperty(ref this.guestNewPassword, value, this.RaiseCanExecuteChanged);
        }

        public string GuestNewPasswordConfirm
        {
            get => this.guestNewPasswordConfirm;
            set => this.SetProperty(ref this.guestNewPasswordConfirm, value, this.RaiseCanExecuteChanged);
        }

        public bool InstallerActive
        {
            get => this.installerActive;
            set
            {
                this.SetProperty(ref this.installerActive, value, this.RaiseCanExecuteChanged);

                if (value)
                {
                    this.MovementActive = false;
                    this.OperatorActive = false;
                    this.GuestActive = false;
                }
            }
        }

        public ICommand InstallerCommand =>
           this.installerCommand
           ??
           (this.installerCommand = new DelegateCommand(
               () => this.InstallerActive = true));

        public string InstallerNewPassword
        {
            get => this.installerNewPassword;
            set => this.SetProperty(ref this.installerNewPassword, value, this.RaiseCanExecuteChanged);
        }

        public string InstallerNewPasswordConfirm
        {
            get => this.installerNewPasswordConfirm;
            set => this.SetProperty(ref this.installerNewPasswordConfirm, value, this.RaiseCanExecuteChanged);
        }

        public bool IsEnabledEditing
        {
            get => this.isEnabledEditing;
            set => this.SetProperty(ref this.isEnabledEditing, value, this.RaiseCanExecuteChanged);
        }

        public bool IsGuestEnabled
        {
            get => this.isGuestEnabled;
            set => this.SetProperty(ref this.isGuestEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsMovementEnabled
        {
            get => this.isMovementEnabled;
            set => this.SetProperty(ref this.isMovementEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOperatorEnabledWithWMS
        {
            get => this.isOperatorEnabledWithWMS;
            set => this.SetProperty(ref this.isOperatorEnabledWithWMS, value, this.RaiseCanExecuteChanged);
        }

        public bool MovementActive
        {
            get => this.movementActive;
            set
            {
                this.SetProperty(ref this.movementActive, value, this.RaiseCanExecuteChanged);

                if (value)
                {
                    this.InstallerActive = false;
                    this.OperatorActive = false;
                    this.GuestActive = false;
                }
            }
        }

        public ICommand MovementCommand =>
           this.movementCommand
           ??
           (this.movementCommand = new DelegateCommand(
               () => this.MovementActive = true));

        public string MovementNewPassword
        {
            get => this.movementNewPassword;
            set => this.SetProperty(ref this.movementNewPassword, value, this.RaiseCanExecuteChanged);
        }

        public string MovementNewPasswordConfirm
        {
            get => this.movementNewPasswordConfirm;
            set => this.SetProperty(ref this.movementNewPasswordConfirm, value, this.RaiseCanExecuteChanged);
        }

        public bool OperatorActive
        {
            get => this.operatorActive;
            set
            {
                this.SetProperty(ref this.operatorActive, value, this.RaiseCanExecuteChanged);

                if (value)
                {
                    this.InstallerActive = false;
                    this.MovementActive = false;
                    this.GuestActive = false;
                }
            }
        }

        public ICommand OperatorCommand =>
           this.operatorCommand
           ??
           (this.operatorCommand = new DelegateCommand(
               () => this.OperatorActive = true));

        public string OperatorNewPassword
        {
            get => this.operatorNewPassword;
            set => this.SetProperty(ref this.operatorNewPassword, value, this.RaiseCanExecuteChanged);
        }

        public string OperatorNewPasswordConfirm
        {
            get => this.operatorNewPasswordConfirm;
            set => this.SetProperty(ref this.operatorNewPasswordConfirm, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveIsGuestEnabledCommand =>
          this.saveIsGuestEnabledCommand
          ??
          (this.saveIsGuestEnabledCommand = new DelegateCommand(
              async () => await this.SaveIsGuestEnabled(),
              this.CanEnableGuestCommand));

        public ICommand SaveIsMovementEnabledCommand =>
                  this.saveIsMovementEnabledCommand
          ??
          (this.saveIsMovementEnabledCommand = new DelegateCommand(
              async () => await this.SaveIsMovementEnabled(),
              this.CanEnableMovementCommand));

        public ICommand SaveIsOperatorEnabledWithWMSCommand =>
                  this.saveIsOperatorEnabledWithWMSCommand
          ??
          (this.saveIsOperatorEnabledWithWMSCommand = new DelegateCommand(
              async () => await this.SaveIsOperatorEnabledWithWMS(),
              this.CanExecute));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.OperatorNewPasswordConfirm = string.Empty;
            this.OperatorNewPassword = string.Empty;
            this.InstallerNewPasswordConfirm = string.Empty;
            this.InstallerNewPassword = string.Empty;
            this.MovementNewPassword = string.Empty;
            this.MovementNewPasswordConfirm = string.Empty;

            this.InstallerActive = true;
            this.OperatorActive = false;
            this.MovementActive = false;
            this.GuestActive = false;
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsEnabledEditing = true;

            this.isWmsEnabled = await this.wmsStatusWebService.IsEnabledAsync();

            this.IsOperatorEnabledWithWMS = await this.usersService.GetOperatorEnabledWithWMSAsync();

            this.IsKeyboardButtonVisible = await this.machineIdentityWebService.GetTouchHelperEnableAsync();

            this.IsMovementEnabled = !await this.usersService.GetIsDisabledAsync("movement");
            this.IsGuestEnabled = !await this.usersService.GetIsDisabledAsync("guest");

            this.InstallerActive = true;
        }

        public void SelectedUser(bool userActive)
        {
            userActive = true;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.changeOperatorPasswordCommand?.RaiseCanExecuteChanged();
            this.changeInstallerPasswordCommand?.RaiseCanExecuteChanged();
            this.changeMovementPasswordCommand?.RaiseCanExecuteChanged();
            this.changeGuestPasswordCommand?.RaiseCanExecuteChanged();
            this.saveIsOperatorEnabledWithWMSCommand?.RaiseCanExecuteChanged();
            this.saveIsMovementEnabledCommand?.RaiseCanExecuteChanged();
            this.saveIsGuestEnabledCommand?.RaiseCanExecuteChanged();
        }

        private bool CanEnableGuestCommand()
        {
            return this.isEnabledEditing &&
                this.sessionService.UserAccessLevel >= UserAccessLevel.Installer;
        }

        private bool CanEnableMovementCommand()
        {
            return this.isEnabledEditing &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Installer;
        }

        private bool CanExecute()
        {
            return this.isEnabledEditing && this.isWmsEnabled &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
        }

        private bool CanExecuteGuestCommand()
        {
            return this.isEnabledEditing &&
                !string.IsNullOrEmpty(this.guestNewPasswordConfirm) &&
                !string.IsNullOrEmpty(this.guestNewPassword) &&
                this.guestNewPasswordConfirm == this.guestNewPassword &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
        }

        private bool CanExecuteInstallerCommand()
        {
            return this.isEnabledEditing &&
                !string.IsNullOrEmpty(this.installerNewPasswordConfirm) &&
                !string.IsNullOrEmpty(this.installerNewPassword) &&
                this.installerNewPasswordConfirm == this.installerNewPassword &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Installer;
        }

        private bool CanExecuteMovementCommand()
        {
            return this.isEnabledEditing &&
                !string.IsNullOrEmpty(this.movementNewPasswordConfirm) &&
                !string.IsNullOrEmpty(this.movementNewPassword) &&
                this.movementNewPasswordConfirm == this.movementNewPassword &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement &&
                this.sessionService.UserAccessLevel != UserAccessLevel.Installer;
        }

        private bool CanExecuteOperatorCommand()
        {
            return this.isEnabledEditing &&
                !string.IsNullOrEmpty(this.operatorNewPasswordConfirm) &&
                !string.IsNullOrEmpty(this.operatorNewPassword) &&
                this.operatorNewPasswordConfirm == this.operatorNewPassword &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
        }

        private async Task ChangePassword(string name)
        {
            try
            {
                this.IsEnabledEditing = false;
                var newPassword = string.Empty;

                switch (name)
                {
                    case "installer":
                        newPassword = this.installerNewPassword;
                        break;

                    case "operator":
                        newPassword = this.operatorNewPassword;
                        break;

                    case "movement":
                        newPassword = this.movementNewPassword;
                        break;

                    case "guest":
                        newPassword = this.guestNewPassword;
                        break;
                }

                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < MinimumPasswordLength)
                {
                    this.ShowNotification(LoadLogin.PasswordIsTooShort, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    await this.usersService.ChangePasswordAsync(name, newPassword);
                    this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsEnabledEditing = true;
            }
        }

        private async Task SaveIsGuestEnabled()
        {
            try
            {
                this.IsEnabledEditing = false;
                await this.usersService.SetIsDisabledAsync("guest", !this.IsGuestEnabled);
                this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsEnabledEditing = true;
            }
        }

        private async Task SaveIsMovementEnabled()
        {
            try
            {
                this.IsEnabledEditing = false;
                await this.usersService.SetIsDisabledAsync("movement", !this.IsMovementEnabled);
                this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsEnabledEditing = true;
            }
        }

        private async Task SaveIsOperatorEnabledWithWMS()
        {
            try
            {
                this.IsEnabledEditing = false;
                await this.usersService.SetOperatorEnabledWithWMSAsync(this.IsOperatorEnabledWithWMS);
                this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsEnabledEditing = true;
            }
        }

        #endregion
    }
}
