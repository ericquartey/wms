using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class DateTimeViewModel : BaseMainViewModel
    {
        #region Fields

        private bool canGoAutoSync;

        private ushort? day;

        private ushort? hour;

        private bool isAuto;

        private bool isBusy;

        private ushort? minute;

        private ushort? month;

        private DelegateCommand saveCommand;

        private ushort? year;

        #endregion

        #region Constructors

        public DateTimeViewModel()
            : base(PresentationMode.Installer)
        {
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
                    ((DelegateCommand)this.saveCommand).RaiseCanExecuteChanged();
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
                    ((DelegateCommand)this.saveCommand).RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
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

        public override Task OnAppearedAsync()
        {
            this.SetTime();
            this.IsAuto = true;
            this.CanGoAutoSync = true;

            this.RaisePropertyChanged();

            return base.OnAppearedAsync();
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private bool IsDateTimeValid()
        {
            var dateToCheck = $"{this.day}/{this.Month}/{this.Year}";
            var formats = new[] { "dd/MM/yyyy" };
            if (DateTime.TryParseExact(dateToCheck, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDateValue))
            {
                return true;
            }

            return false;
        }

        private async Task SaveAsync()
        {
            try
            {
                if (!this.IsDateTimeValid())
                {
                    this.ShowNotification("Date/time is invalid", Services.Models.NotificationSeverity.Warning);
                    return;
                }

                this.IsBusy = true;

                this.ClearNotifications();

                this.IsBackNavigationAllowed = false;

                // TO DO set current system date time

                this.ShowNotification(InstallationApp.SaveSuccessful);

                this.IsBusy = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                this.IsBackNavigationAllowed = true;
            }
        }

        private void SetTime()
        {
            var currentDateTime = DateTime.Now;

            this.Minute = (ushort)currentDateTime.Minute;
            this.Hour = (ushort)currentDateTime.Hour;
            this.Day = (ushort)currentDateTime.Day;
            this.Month = (ushort)currentDateTime.Month;
            this.Year = (ushort)currentDateTime.Year;
        }

        #endregion
    }
}
