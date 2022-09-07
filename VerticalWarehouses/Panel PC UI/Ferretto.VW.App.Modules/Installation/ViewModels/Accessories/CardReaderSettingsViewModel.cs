using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CardReaderSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly ObservableCollection<string> acquiredTokens = new ObservableCollection<string>();

        private readonly ICardReaderService cardReaderService;

        private readonly IMachineUsersWebService machineUsersWebService;

        private readonly IDialogService dialogService;

        private readonly ObservableCollection<InputKey> inputKeys = new ObservableCollection<InputKey>();

        private ObservableCollection<UserParameters> users = new ObservableCollection<UserParameters>();

        private readonly EventHandler<string> keyAcquiredEventHandler;

        private readonly EventHandler<RegexMatchEventArgs> tokenAcquiredEventHandler;

        private bool areEventsRegistered;

        private bool isTesting;

        private bool isLocal;

        private DelegateCommand startTestCommand;

        private DelegateCommand stopTestCommand;

        private DelegateCommand addCommand;

        private DelegateCommand deleteCommand;

        private string tokenRegex;

        private string userName;

        private bool canEditTokenRegex;

        private UserParameters selectedUser;

        #endregion

        #region Constructors

        public CardReaderSettingsViewModel(ICardReaderService cardReaderService,
            IMachineUsersWebService machineUsersWebService,
            IDialogService dialogService)
        {
            this.cardReaderService = cardReaderService;
            this.machineUsersWebService = machineUsersWebService;
            this.dialogService = dialogService;

            this.tokenAcquiredEventHandler = new EventHandler<RegexMatchEventArgs>(this.OnTokenAcquiredAsync);
            this.keyAcquiredEventHandler = new EventHandler<string>(this.OnKeyAcquired);
        }

        #endregion

        #region Properties

        public ObservableCollection<string> AcquiredTokens => this.acquiredTokens;

        public IEnumerable<InputKey> InputKeys => this.inputKeys;

        public bool IsTesting
        {
            get => this.isTesting;
            set
            {
                if (this.SetProperty(ref this.isTesting, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public UserParameters SelectedUser
        {
            get => this.selectedUser;
            set => this.SetProperty(ref this.selectedUser, value, this.RaiseCanExecuteChanged);
        }

        public bool CanEditTokenRegex
        {
            get => this.canEditTokenRegex;
            set => this.SetProperty(ref this.canEditTokenRegex, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLocal
        {
            get => this.isLocal;
            set
            {
                if (this.SetProperty(ref this.isLocal, value, this.RaiseCanExecuteChanged))
                {
                    this.AreSettingsChanged = true;
                }
            }
        }

        public ObservableCollection<UserParameters> Users
        {
            get => this.users;
            set => this.SetProperty(ref this.users, value, this.RaiseCanExecuteChanged);
        }

        public string UserName
        {
            get => this.userName;
            set => this.SetProperty(ref this.userName, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StartTestCommand =>
            this.startTestCommand
            ??
            (this.startTestCommand = new DelegateCommand(
                async () =>
                await this.StartTestAsync(),
                this.CanStartTest));

        public ICommand AddCommand =>
            this.addCommand
            ??
            (this.addCommand = new DelegateCommand(
                async () =>
                await this.AddAsync(),
                this.CanAdd));

        public ICommand DeleteCommand =>
            this.deleteCommand
            ??
            (this.deleteCommand = new DelegateCommand(
                async () =>
                await this.DeleteAsync(),
                this.CanDelete));

        public ICommand StopTestCommand =>
           this.stopTestCommand
           ??
           (this.stopTestCommand = new DelegateCommand(
               async () =>
               await this.StopTestAsync(),
               this.CanStopTest));

        public string TokenRegex
        {
            get => this.tokenRegex;
            set
            {
                if (this.SetProperty(ref this.tokenRegex, value))
                {
                    this.AreSettingsChanged = true;
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.StopTestAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            if (this.areEventsRegistered)
            {
                this.cardReaderService.TokenAcquired -= this.tokenAcquiredEventHandler;
                this.cardReaderService.KeysAcquired -= this.keyAcquiredEventHandler;
                this.areEventsRegistered = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (!this.areEventsRegistered)
            {
                this.cardReaderService.TokenAcquired += this.tokenAcquiredEventHandler;
                this.cardReaderService.KeysAcquired += this.keyAcquiredEventHandler;
                this.areEventsRegistered = true;
            }

            await this.RefreshTokenUsers();
        }

        private async Task RefreshTokenUsers()
        {
            this.Users.Clear();

            var allUsers = await this.machineUsersWebService.GetAllTokenUsersAsync();
            this.Users.AddRange(allUsers.Where(u => !string.IsNullOrEmpty(u.Token)));
        }

        protected override async Task TestAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                //Do nothing
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

        public bool CanAdd()
        {
            return this.UserName != null && this.AcquiredTokens.Any();
        }

        public async Task AddAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.ClearNotifications();
                if (this.Users.Any(u => u.Name == this.UserName))
                {
                    this.ShowNotification(InstallationApp.NameIsPresent, Services.Models.NotificationSeverity.Error);
                }
                else if (this.Users.Any(u => u.Token == this.AcquiredTokens.LastOrDefault()))
                {
                    this.ShowNotification(InstallationApp.TokenIsPresent, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    var userParameters = new UserParameters() { Name = this.UserName, AccessLevel = ((int)UserAccessLevel.Operator), PasswordHash = "", PasswordSalt = "", Token = this.AcquiredTokens.LastOrDefault() };

                    await this.machineUsersWebService.AddUserAsync(userParameters);

                    this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);

                    await this.RefreshTokenUsers();
                }
                this.AcquiredTokens.Clear();
                this.UserName = string.Empty;
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

        public bool CanDelete()
        {
            return this.SelectedUser != null && !string.IsNullOrEmpty(this.SelectedUser.Token);
        }

        public async Task DeleteAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.DeleteUserConfirm"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult is DialogResult.Yes)
                {
                    await this.machineUsersWebService.DeleteUserAsync(this.SelectedUser);
                    this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
                    await this.RefreshTokenUsers();
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

        protected override bool CanSave() =>
            base.CanSave()
            &&
            !this.isTesting;

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            this.inputKeys.Clear();
            this.acquiredTokens.Clear();
            this.IsTesting = false;

            try
            {
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories)
                {
                    this.IsAccessoryEnabled = bayAccessories.CardReader.IsEnabledNew;
                    this.IsLocal = bayAccessories.CardReader.IsLocal is true;
                    this.TokenRegex = bayAccessories.CardReader.TokenRegex ?? "(?<Token>\\d+)";
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the card reader settings page. Leaving the page ...");

                    this.NavigationService.GoBack();
                }

                this.AreSettingsChanged = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.CanEditTokenRegex = !this.IsTesting && !this.IsWaitingForResponse && this.IsAccessoryEnabled;

            base.RaiseCanExecuteChanged();

            this.stopTestCommand?.RaiseCanExecuteChanged();
            this.startTestCommand?.RaiseCanExecuteChanged();
            this.addCommand?.RaiseCanExecuteChanged();
            this.deleteCommand?.RaiseCanExecuteChanged();
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.Logger.Debug("Saving card reader settings ...");

                this.IsWaitingForResponse = true;
                await this.cardReaderService.UpdateSettingsAsync(this.IsAccessoryEnabled, this.TokenRegex, this.IsLocal);

                this.Logger.Debug("Card reader settings saved.");
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

        private bool CanStartTest() =>
            !this.IsWaitingForResponse
            &&
            !this.IsTesting
            &&
            this.IsAccessoryEnabled;

        private bool CanStopTest() => this.IsTesting;

        private void OnKeyAcquired(object sender, string e)
        {
            this.inputKeys.Add(new InputKey(e));
        }

        private async void OnTokenAcquiredAsync(object sender, RegexMatchEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.acquiredTokens.Add(e.Token);
            });

            this.ShowNotification(VW.App.Resources.InstallationApp.CodeRecognized, Services.Models.NotificationSeverity.Success);
            if (this.isLocal)
            {
                await this.StopTestAsync();
            }
        }

        private async Task StartTestAsync()
        {
            if (this.AreSettingsChanged)
            {
                this.ShowNotification(
                    VW.App.Resources.InstallationApp.SaveChangesBeforeStartingTest,
                    Services.Models.NotificationSeverity.Warning);

                return;
            }

            try
            {
                this.IsTesting = true;
                this.inputKeys.Clear();
                this.acquiredTokens.Clear();

                await this.cardReaderService.StartAsync();
                this.ShowNotification(VW.App.Resources.InstallationApp.UseTheCardOnCardReader);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.IsTesting = false;
            }
        }

        private async Task StopTestAsync()
        {
            try
            {
                this.IsTesting = false;

                this.ClearNotifications();

                await this.cardReaderService.StopAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
