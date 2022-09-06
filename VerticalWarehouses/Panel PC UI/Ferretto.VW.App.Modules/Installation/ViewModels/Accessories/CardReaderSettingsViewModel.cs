using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CardReaderSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly ObservableCollection<string> acquiredTokens = new ObservableCollection<string>();

        private readonly ICardReaderService cardReaderService;

        private readonly ObservableCollection<InputKey> inputKeys = new ObservableCollection<InputKey>();

        private readonly EventHandler<string> keyAcquiredEventHandler;

        private readonly EventHandler<RegexMatchEventArgs> tokenAcquiredEventHandler;

        private bool areEventsRegistered;

        private bool isTesting;

        private DelegateCommand startTestCommand;

        private DelegateCommand stopTestCommand;

        private string tokenRegex;

        private bool canEditTokenRegex;

        #endregion

        #region Constructors

        public CardReaderSettingsViewModel(ICardReaderService cardReaderService)
        {
            this.cardReaderService = cardReaderService;

            this.tokenAcquiredEventHandler = new EventHandler<RegexMatchEventArgs>(this.OnTokenAcquired);
            this.keyAcquiredEventHandler = new EventHandler<string>(this.OnKeyAcquired);
        }

        #endregion

        #region Properties

        public IEnumerable<string> AcquiredTokens => this.acquiredTokens;

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

        public bool CanEditTokenRegex
        {
            get => this.canEditTokenRegex;
            set => this.SetProperty(ref this.canEditTokenRegex, value, this.RaiseCanExecuteChanged);
        }

        public ICommand StartTestCommand =>
            this.startTestCommand
            ??
            (this.startTestCommand = new DelegateCommand(
                async () =>
                await this.StartTestAsync(),
                this.CanStartTest));

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

        private void OnTokenAcquired(object sender, RegexMatchEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.acquiredTokens.Add(e.Token);
            });

            this.ShowNotification(VW.App.Resources.InstallationApp.CodeRecognized, Services.Models.NotificationSeverity.Success);
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
