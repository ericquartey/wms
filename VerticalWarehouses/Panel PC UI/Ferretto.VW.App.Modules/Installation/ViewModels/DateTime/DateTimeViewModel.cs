using System;
using System.Globalization;
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
    internal class DateTimeViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineUtcTimeWebService machineUtcTimeWebService;

        private bool canGoAutoSync;

        private ushort? day;

        private ushort? hour;

        private bool isAuto;

        private bool isBusy;

        private bool isManual;

        private bool isManualEnabled;

        private ushort? minute;

        private ushort? month;

        private DelegateCommand saveCommand;

        private ushort? year;

        #endregion

        #region Constructors

        public DateTimeViewModel(IMachineUtcTimeWebService machineUtcTimeWebService)
            : base(PresentationMode.Installer)
        {
            this.machineUtcTimeWebService = machineUtcTimeWebService ?? throw new ArgumentNullException(nameof(machineUtcTimeWebService));

            this.IsAuto = true;
        }

        #endregion

        #region Properties

        public bool CanGoAutoSync
        {
            get => this.canGoAutoSync;
            set
            {
                if (this.SetProperty(ref this.canGoAutoSync, value))
                {
                    if (!this.canGoAutoSync)
                    {
                        this.IsAuto = false;
                    }
                }
            }
        }

        public ushort? Day
        {
            get => this.day;
            set => this.SetProperty(ref this.day, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public ushort? Hour
        {
            get => this.hour;
            set => this.SetProperty(ref this.hour, value);
        }

        public bool IsAuto
        {
            get => this.isAuto;
            set
            {
                if (this.SetProperty(ref this.isAuto, value))
                {
                    this.isManual = !this.isAuto;
                    ((DelegateCommand)this.saveCommand)?.RaiseCanExecuteChanged();
                    this.RaisePropertyChanged(nameof(this.IsManual));
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    ((DelegateCommand)this.saveCommand)?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public bool IsManual
        {
            get => this.isManual;
            set
            {
                if (this.SetProperty(ref this.isManual, value))
                {
                    this.isAuto = !this.isManual;
                    ((DelegateCommand)this.saveCommand)?.RaiseCanExecuteChanged();
                    this.RaisePropertyChanged(nameof(this.IsAuto));
                }
            }
        }

        public bool IsManualEnabled
        {
            get => this.isManualEnabled;
            set
            {
                if (this.SetProperty(ref this.isManualEnabled, value))
                {
                    ((DelegateCommand)this.saveCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ushort? Minute
        {
            get => this.minute;
            set => this.SetProperty(ref this.minute, value);
        }

        public ushort? Month
        {
            get => this.month;
            set => this.SetProperty(ref this.month, value);
        }

        public ICommand SaveCommand =>
                               this.saveCommand
               ??
               (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public ushort? Year
        {
            get => this.year;
            set => this.SetProperty(ref this.year, value);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.GetTimeAsync();
        }

        private bool CanSave()
        {
            return !this.IsBusy
                   &&
                   (this.IsAuto
                   ||
                   (!this.IsAuto && this.IsManualEnabled));
        }

        private DateTimeOffset? GetNewDateTime()
        {
            if (this.year >= 2020 && this.year <= 3000
                && this.month >= 1 && this.month <= 12
                && this.day >= 1 && this.day <= DateTime.DaysInMonth(this.year.Value, this.month.Value)
                && this.hour >= 0 && this.hour <= 23
                && this.minute >= 0 && this.minute <= 59)
            {
                var daTe = new DateTime(this.year.Value, this.month.Value, this.day.Value, this.hour.Value, this.minute.Value, 0, DateTimeKind.Local);
                daTe.ToUniversalTime();
                return new DateTimeOffset(daTe.ToUniversalTime());
            }

            return null;
        }

        private async Task GetTimeAsync()
        {
            DateTimeOffset? currentDateTime = null;
            try
            {
                this.CanGoAutoSync = await this.machineUtcTimeWebService.CanEnableWmsAutoSyncModeAsync();
                this.IsManualEnabled = true;

                if (this.canGoAutoSync)
                {
                    this.IsAuto = await this.machineUtcTimeWebService.IsWmsAutoSyncEnabledAsync();
                }

                this.IsManual = !this.IsAuto;

                var newcurrentDateTime = await this.machineUtcTimeWebService.GetAsync();
                currentDateTime = newcurrentDateTime.ToLocalTime();
            }
            catch (Exception ex)
            {
                this.canGoAutoSync = false;
                this.IsAuto = false;
                this.IsManualEnabled = false;
                this.ShowNotification(ex);
            }
            finally
            {
                if (!(currentDateTime is null))
                {
                    this.Minute = (ushort)currentDateTime.Value.Minute;
                    this.Hour = (ushort)currentDateTime.Value.Hour;
                    this.Day = (ushort)currentDateTime.Value.Day;
                    this.Month = (ushort)currentDateTime.Value.Month;
                    this.Year = (ushort)currentDateTime.Value.Year;
                }
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var newDateTime = this.GetNewDateTime();
                if (newDateTime is null
                    &&
                    !this.isAuto)
                {
                    this.ShowNotification(InstallationApp.DateTimeEnteredIsInvalid, Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsBusy = true;

                this.ClearNotifications();

                await this.machineUtcTimeWebService.SetWmsAutoSyncAsync(this.IsAuto);

                if (!this.isAuto)
                {
                    await this.machineUtcTimeWebService.SetAsync(newDateTime.Value);
                }

                this.ShowNotification(InstallationApp.SaveSuccessful);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        #endregion
    }
}
