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

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private readonly IMachineUsersWebService usersService;

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private DelegateCommand changeInstallerPasswordCommand;

        private DelegateCommand changeMovementPasswordCommand;

        private DelegateCommand changeOperatorPasswordCommand;

        private string installerNewPassword;

        private string installerNewPasswordConfirm;

        private bool isEnabledEditing;

        private bool isMovementEnabled;

        private bool isOperatorEnabledWithWMS;

        private bool isWmsEnabled;

        private string movementNewPassword;

        private string movementNewPasswordConfirm;

        private string operatorNewPassword;

        private string operatorNewPasswordConfirm;

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

        public ICommand ChangeInstallerPasswordCommand =>
           this.changeInstallerPasswordCommand
           ??
           (this.changeInstallerPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword(UserAccessLevel.Installer),
               this.CanExecuteInstallerCommand));

        public ICommand ChangeMovementPasswordCommand =>
           this.changeMovementPasswordCommand
           ??
           (this.changeMovementPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword(UserAccessLevel.Movement),
               this.CanExecuteMovementCommand));

        public ICommand ChangeOperatorPasswordCommand =>
                   this.changeOperatorPasswordCommand
           ??
           (this.changeOperatorPasswordCommand = new DelegateCommand(
               async () => await this.ChangePassword(UserAccessLevel.Operator),
               this.CanExecuteOperatorCommand));

        public override EnableMask EnableMask => EnableMask.Any;

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

        public ICommand SaveIsMovementEnabledCommand =>
          this.saveIsMovementEnabledCommand
          ??
          (this.saveIsMovementEnabledCommand = new DelegateCommand(
              async () => await this.SaveIsMovementEnabled(),
              this.CanExecuteInstallerCommand));

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
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.changeOperatorPasswordCommand?.RaiseCanExecuteChanged();
            this.changeInstallerPasswordCommand?.RaiseCanExecuteChanged();
            this.changeMovementPasswordCommand?.RaiseCanExecuteChanged();
            this.saveIsOperatorEnabledWithWMSCommand?.RaiseCanExecuteChanged();
            this.saveIsMovementEnabledCommand?.RaiseCanExecuteChanged();
        }

        private bool CanExecute()
        {
            return this.isEnabledEditing &&
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
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement;
        }

        private bool CanExecuteOperatorCommand()
        {
            return this.isEnabledEditing &&
                !string.IsNullOrEmpty(this.operatorNewPasswordConfirm) &&
                !string.IsNullOrEmpty(this.operatorNewPassword) &&
                this.operatorNewPasswordConfirm == this.operatorNewPassword &&
                this.sessionService.UserAccessLevel > UserAccessLevel.Movement &&
                !this.isWmsEnabled;
        }

        private async Task ChangePassword(UserAccessLevel userAccessLevel)
        {
            try
            {
                this.IsEnabledEditing = false;

                if (userAccessLevel == UserAccessLevel.Installer)
                {
                    await this.usersService.ChangePasswordAsync("installer", this.installerNewPassword);
                    this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
                }
                else if (userAccessLevel == UserAccessLevel.Operator)
                {
                    await this.usersService.ChangePasswordAsync("operator", this.operatorNewPassword);
                    this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
                }
                else if (userAccessLevel == UserAccessLevel.Movement)
                {
                    await this.usersService.ChangePasswordAsync("movement", this.movementNewPassword);
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
