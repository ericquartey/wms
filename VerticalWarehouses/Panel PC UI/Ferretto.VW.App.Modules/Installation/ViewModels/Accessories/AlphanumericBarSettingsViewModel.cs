using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class AlphaNumericBarSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IAlphaNumericBarDriver deviceDriver;

        private readonly IAlphaNumericBarService deviceService;

        private DelegateCommand addFieldsCommand;

        private ObservableCollection<string> allTypeFields = new ObservableCollection<string>() { "ItemCode", "ItemDescription", "Destination", "ItemListCode", "ItemListDescription", "ItemListRowCode", "ItemNotes", "Lot", "SerialNumber", "Sscc", };

        private bool clearOnClose;

        private bool hasGetErrors;

        private int incrementTimeLoop = 0;

        private IPAddress ipAddress;

        private bool loopTestIsChecked;

        private string loopTextMessage;

        private Timer loopTimer;

        private int maxMessageLength;

        private List<string> messageFields = new List<string>();

        private int port;

        private DelegateCommand resetFieldsCommand;

        private string selectedField;

        private AlphaNumericBarSize size;

        private bool testArrowIsChecked;

        private int testArrowOffset;

        private bool testLedIsChecked;

        private bool testMessageIsChecked;

        private int testMessageOffset;

        private string testMessageText;

        private bool testOffIsChecked;

        private bool useGet;

        #endregion

        #region Constructors

        public AlphaNumericBarSettingsViewModel(
            IBayManager bayManager,
            IAlphaNumericBarDriver deviceDriver,
            IAlphaNumericBarService alphaNumericBarService)
        {
            this.bayManager = bayManager;
            this.deviceDriver = deviceDriver;
            this.deviceService = alphaNumericBarService;
        }

        #endregion

        #region Properties

        public ICommand AddFieldsCommand => this.addFieldsCommand
           ??
           (this.addFieldsCommand =
               new DelegateCommand(
                   () => this.AddFields(),
                   this.CanAddFields));

        public ObservableCollection<string> AllTypeFields
        {
            get => this.allTypeFields;
            set => this.SetProperty(ref this.allTypeFields, value, this.RaiseCanExecuteChanged);
        }

        public bool ClearOnClose
        {
            get => this.clearOnClose;
            set
            {
                if (this.SetProperty(ref this.clearOnClose, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool HasGetErrors
        {
            get => this.hasGetErrors;
            set => this.SetProperty(ref this.hasGetErrors, value, this.RaiseCanExecuteChanged);
        }

        public IPAddress IpAddress
        {
            get => this.ipAddress;
            set
            {
                if (this.SetProperty(ref this.ipAddress, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsEnabledEditing => true;

        public bool LoopTestIsChecked
        {
            get => this.loopTestIsChecked;
            set
            {
                if (this.SetProperty(ref this.loopTestIsChecked, value))
                {
                    if (value)
                    {
                        this.deviceDriver.HasGetErrors = false;
                        this.deviceDriver.IsTestLoop = true;
                        this.HasGetErrors = false;
                        this.incrementTimeLoop = 1;
                        this.loopTextMessage = this.TestMessageText;
                        this.loopTimer = new Timer(this.CallBackLoop, null, 0, 5000);
                    }
                    else
                    {
                        this.TestMessageText = this.loopTextMessage;
                        this.deviceDriver.IsTestLoop = false;
                        this.ShowNotification(InstallationApp.StopLoop, Services.Models.NotificationSeverity.Success);
                        this.loopTimer?.Change(-1, -1);
                    }
                }
            }
        }

        public int MaxMessageLength
        {
            get => this.maxMessageLength;
            set
            {
                if (this.SetProperty(ref this.maxMessageLength, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public List<string> MessageFields
        {
            get => this.messageFields;
            set
            {
                if (this.SetProperty(ref this.messageFields, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    this.ShowNotification(App.Resources.Localized.Get("OperatorApp.GenericValidationError"), Services.Models.NotificationSeverity.Error);
                    return;
                }

                if (this.SetProperty(ref this.port, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ResetFieldsCommand => this.resetFieldsCommand
           ??
           (this.resetFieldsCommand =
               new DelegateCommand(
                   () => this.ResetFields(),
                   this.CanResetFields));

        public string SelectedField
        {
            get => this.selectedField;
            set => this.SetProperty(ref this.selectedField, value, this.RaiseCanExecuteChanged);
        }

        public AlphaNumericBarSize Size
        {
            get => this.size;
            set
            {
                if (this.SetProperty(ref this.size, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<AlphaNumericBarSize> Sizes => Enum.GetValues(typeof(AlphaNumericBarSize)).OfType<AlphaNumericBarSize>().ToList();

        public bool TestArrowIsChecked
        {
            get => this.testArrowIsChecked;
            set
            {
                if (this.SetProperty(ref this.testArrowIsChecked, value))
                {
                    if (value)
                    {
                        this.StopTestLoop();
                        _ = this.DoTestArrowOnAsync(this.testArrowOffset);
                    }
                }
            }
        }

        public int TestArrowOffset
        {
            get => this.testArrowOffset;
            set
            {
                if (value < 0 || value > 5000)
                {
                    value = 0;
                }

                if (this.SetProperty(ref this.testArrowOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool TestLedIsChecked
        {
            get => this.testLedIsChecked;
            set
            {
                if (this.SetProperty(ref this.testLedIsChecked, value))
                {
                    if (value)
                    {
                        this.StopTestLoop();
                        _ = this.DoTestLedAsync(value);
                    }
                }
            }
        }

        public bool TestMessageIsChecked
        {
            get => this.testMessageIsChecked;
            set
            {
                if (this.SetProperty(ref this.testMessageIsChecked, value))
                {
                    if (value)
                    {
                        this.StopTestLoop();
                        _ = this.DoTestMessageOnAsync(this.TestMessageText, this.testMessageOffset);
                    }
                }
            }
        }

        public int TestMessageOffset
        {
            get => this.testMessageOffset;
            set
            {
                if (value < 0 || value > 5000)
                {
                    value = 0;
                }

                if (this.SetProperty(ref this.testMessageOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string TestMessageText
        {
            get => this.testMessageText;
            set
            {
                if (this.SetProperty(ref this.testMessageText, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool TestOffIsChecked
        {
            get => this.testOffIsChecked;
            set
            {
                if (this.SetProperty(ref this.testOffIsChecked, value))
                {
                    if (value)
                    {
                        this.StopTestLoop();
                        _ = this.DoTestLedAsync(false);
                    }
                }
            }
        }

        public bool UseGet
        {
            get => this.useGet;
            set
            {
                if (this.SetProperty(ref this.useGet, value))
                {
                    {
                        this.AreSettingsChanged = true;
                        this.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
            this.LoopTestIsChecked = false;
        }

        protected override bool CanSave()
        {
            var res = base.CanSave() && this.MessageFields.Any();

            if (!this.MessageFields.Any())
            {
                this.ShowNotification(App.Resources.Localized.Get("InstallationApp.EmptyFields"), Services.Models.NotificationSeverity.Warning);
            }
            else
            {
                this.ClearNotifications();
            }

            return res;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories)
                {
                    this.IsWaitingForResponse = true;

                    this.IsAccessoryEnabled = bayAccessories.AlphaNumericBar.IsEnabledNew;
                    this.IpAddress = bayAccessories.AlphaNumericBar.IpAddress;
                    this.Port = bayAccessories.AlphaNumericBar.TcpPort;
                    this.Size = bayAccessories.AlphaNumericBar.Size;
                    this.MaxMessageLength = bayAccessories.AlphaNumericBar.MaxMessageLength;
                    this.ClearOnClose = bayAccessories.AlphaNumericBar.ClearAlphaBarOnCloseView is true;

                    this.AllTypeFields = new ObservableCollection<string>() { "ItemCode", "ItemDescription", "Destination", "ItemListCode", "ItemListDescription", "ItemListRowCode", "ItemNotes", "Lot", "SerialNumber", "Sscc", };
                    this.MessageFields.Clear();
                    if (!string.IsNullOrEmpty(bayAccessories.AlphaNumericBar.Field1))
                    {
                        this.MessageFields.Add(bayAccessories.AlphaNumericBar.Field1);
                        this.AllTypeFields.Remove(bayAccessories.AlphaNumericBar.Field1);
                    }

                    if (!string.IsNullOrEmpty(bayAccessories.AlphaNumericBar.Field2))
                    {
                        this.MessageFields.Add(bayAccessories.AlphaNumericBar.Field2);
                        this.AllTypeFields.Remove(bayAccessories.AlphaNumericBar.Field2);
                    }

                    if (!string.IsNullOrEmpty(bayAccessories.AlphaNumericBar.Field3))
                    {
                        this.MessageFields.Add(bayAccessories.AlphaNumericBar.Field3);
                        this.AllTypeFields.Remove(bayAccessories.AlphaNumericBar.Field3);
                    }

                    if (!string.IsNullOrEmpty(bayAccessories.AlphaNumericBar.Field4))
                    {
                        this.MessageFields.Add(bayAccessories.AlphaNumericBar.Field4);
                        this.AllTypeFields.Remove(bayAccessories.AlphaNumericBar.Field4);
                    }

                    if (!string.IsNullOrEmpty(bayAccessories.AlphaNumericBar.Field5))
                    {
                        this.MessageFields.Add(bayAccessories.AlphaNumericBar.Field5);
                        this.AllTypeFields.Remove(bayAccessories.AlphaNumericBar.Field5);
                    }

                    this.RaisePropertyChanged(nameof(this.MessageFields));

                    this.UseGet = bayAccessories.AlphaNumericBar.UseGet is true;

                    this.SetDeviceInformation(bayAccessories.AlphaNumericBar.DeviceInformation);

                    this.AreSettingsChanged = false;

                    if (this.TestMessageText is null)
                    {
                        this.TestMessageText = $"{DateTime.Now} {Ferretto.VW.App.Resources.Menu.AccessoriesAlphaNumBarTestMessageDefault}";
                    }

                    this.TestArrowOffset = this.deviceDriver.CalculateOffsetArrowMiddlePosition();
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the alphanumeric settings page. Leaving the page ...");

                    this.NavigationService.GoBack();
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

            await base.OnDataRefreshAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Size));
            this.RaisePropertyChanged(nameof(this.Sizes));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.bayManager.SetAlphaNumericBarAsync(
                    this.IsAccessoryEnabled,
                    this.ipAddress,
                    this.port,
                    this.size,
                    this.maxMessageLength,
                    this.clearOnClose,
                    this.UseGet,
                    this.MessageFields);

                this.deviceDriver.Disconnect();

                await this.deviceService.AlphaNumericBarConfigureAsync();
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

        private void AddFields()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.MessageFields.Add(this.SelectedField);

                this.AllTypeFields.Remove(this.SelectedField);

                this.RaisePropertyChanged(nameof(this.MessageFields));

                this.RaiseCanExecuteChanged();

                this.AreSettingsChanged = true;
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

        private void CallBackLoop(object sender)
        {
            try
            {
                this.TestMessageText = this.incrementTimeLoop++ + ") " + this.loopTextMessage;
                if (!this.deviceDriver.HasGetErrors)
                {
                    _ = this.DoTestMessageOnAsync(this.TestMessageText, this.testMessageOffset);
                }
                else
                {
                    this.loopTestIsChecked = false;
                    this.HasGetErrors = true;
                    this.loopTimer?.Change(-1, -1);
                }
            }
            catch (Exception ex)
            {
                this.loopTestIsChecked = false;
                this.deviceDriver.IsTestLoop = false;
                this.TestMessageText = this.loopTextMessage;
                this.ShowNotification(ex);
            }
        }

        private bool CanAddFields()
        {
            return this.IsEnabled && !string.IsNullOrEmpty(this.SelectedField) && this.MessageFields.Count < 5;
        }

        private bool CanResetFields()
        {
            return this.IsEnabled && this.MessageFields.Any();
        }

        private async Task<bool> DoTestArrowOnAsync(int offset)
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.deviceDriver.EnabledAsync(false);

                return await this.deviceDriver.SetCustomCharacterAsync(0, offset, true);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        private async Task<bool> DoTestLedAsync(bool enable)
        {
            try
            {
                this.IsWaitingForResponse = true;
                return await this.deviceDriver.TestAsync(enable);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        private async Task<bool> DoTestMessageOnAsync(string message, int offset)
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.Logger.Debug($"DoTestMessageOnAsync; message {message}");

                await this.deviceDriver.EnabledAsync(false);

                //if (this.deviceDriver.TestEnabled)
                //{
                //    await this.deviceDriver.TestAsync(false);
                //}

                this.deviceDriver.GetOffsetArrowAndMessage(offset, message, out var offsetArrow, out var offsetMessage, out var scrollEnd);
                await this.deviceDriver.SetAndWriteArrowAsync(offsetArrow, true);
                if (scrollEnd > 0)
                {
                    return await this.deviceDriver.SetAndWriteMessageScrollAsync(message, offsetMessage, scrollEnd, false);
                }
                else
                {
                    return await this.deviceDriver.SetAndWriteMessageAsync(message, offsetMessage, false);
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

            return false;
        }

        private void ResetFields()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.MessageFields.Clear();

                this.AllTypeFields = new ObservableCollection<string>() { "ItemCode", "ItemDescription", "Destination", "ItemListCode", "ItemListDescription", "ItemListRowCode", "ItemNotes", "Lot", "SerialNumber", "Sscc", };

                this.RaisePropertyChanged(nameof(this.MessageFields));

                this.RaiseCanExecuteChanged();

                this.AreSettingsChanged = true;
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

        private void StopTestLoop()
        {
            if (!string.IsNullOrEmpty(this.loopTextMessage))
            {
                this.TestMessageText = this.loopTextMessage;
            }
            this.HasGetErrors = false;
            this.deviceDriver.IsTestLoop = false;
            this.loopTimer?.Change(-1, -1);
        }

        #endregion
    }
}
