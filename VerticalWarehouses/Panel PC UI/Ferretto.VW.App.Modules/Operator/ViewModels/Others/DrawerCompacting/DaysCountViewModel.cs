using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class DaysCountViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand deleteCommand;

        private bool haveBay2;

        private bool haveBay3;

        private bool isBay1;

        private bool isBay2;

        private bool isBay3;

        private bool isBusy;

        private bool isInstaller;

        private ObservableCollection<RotationClassSchedule> rotationClassSchedules = new ObservableCollection<RotationClassSchedule>();

        private DelegateCommand saveCommand;

        private DelegateCommand saveRotationClassCommand;

        private BayNumber selectedBayNumber;

        private RotationClassSchedule selectedRotationClassSchedule;

        private bool singleBay;

        #endregion

        #region Constructors

        public DaysCountViewModel(IMachineIdentityWebService machineIdentityWebService,
            IBayManager bayManagerService,
            IMachineBaysWebService machineBaysWebService)
            : base(PresentationMode.Operator)
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.bayManagerService = bayManagerService ?? throw new ArgumentNullException(nameof(bayManagerService));

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Properties

        public ICommand DeleteCommand =>
                                           this.deleteCommand
           ??
           (this.deleteCommand = new DelegateCommand(
               async () => await this.DeleteAsync(), this.CanDelete));

        public bool HaveBay2
        {
            get => this.haveBay2;
            set => this.SetProperty(ref this.haveBay2, value, this.RaiseCanExecuteChanged);
        }

        public bool HaveBay3
        {
            get => this.haveBay3;
            set => this.SetProperty(ref this.haveBay3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBay1
        {
            get => this.isBay1;
            set
            {
                this.SetProperty(ref this.isBay1, value, this.RaiseCanExecuteChanged);
                if (value)
                {
                    this.selectedBayNumber = BayNumber.BayOne;
                }
            }
        }

        public bool IsBay2
        {
            get => this.isBay2;
            set
            {
                this.SetProperty(ref this.isBay2, value, this.RaiseCanExecuteChanged);
                if (value)
                {
                    this.selectedBayNumber = BayNumber.BayTwo;
                }
            }
        }

        public bool IsBay3
        {
            get => this.isBay3;
            set
            {
                this.SetProperty(ref this.isBay3, value, this.RaiseCanExecuteChanged);
                if (value)
                {
                    this.selectedBayNumber = BayNumber.BayThree;
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInstaller
        {
            get => this.isInstaller;
            set => this.SetProperty(ref this.isInstaller, value, this.RaiseCanExecuteChanged);
        }

        public ObservableCollection<RotationClassSchedule> RotationClassSchedules
        {
            get => this.rotationClassSchedules;
            set => this.SetProperty(ref this.rotationClassSchedules, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
            this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
               async () => await this.SaveAsync(), this.CanSave));

        public ICommand SaveRotationClassCommand =>
            this.saveRotationClassCommand
           ??
           (this.saveRotationClassCommand = new DelegateCommand(
               async () => await this.SaveRotationClassAsync(), this.CanSaveRotationClass));

        public RotationClassSchedule SelectedRotationClassSchedule
        {
            get => this.selectedRotationClassSchedule;
            set => this.SetProperty(ref this.selectedRotationClassSchedule, value, this.RaiseCanExecuteChanged);
        }

        public bool SingleBay
        {
            get => this.singleBay;
            set => this.SetProperty(ref this.singleBay, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.SelectedRotationClassSchedule = null;
            this.RotationClassSchedules.Clear();

            this.IsBusy = false;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsInstaller = this.sessionService.UserAccessLevel > UserAccessLevel.Movement;

            await this.LoadSettings();

            this.IsBusy = false;

            this.selectedBayNumber = BayNumber.None;

            var allBays = await this.machineBaysWebService.GetAllAsync();

            this.selectedBayNumber = allBays.First(b => b.RotationClass == "A").Number;

            switch (this.selectedBayNumber)
            {
                case BayNumber.BayOne:
                    this.IsBay1 = true;
                    break;

                case BayNumber.BayTwo:
                    this.IsBay2 = true;
                    break;

                case BayNumber.BayThree:
                    this.IsBay3 = true;
                    break;
            }

            this.HaveBay2 = allBays.Any(b => b.Number == BayNumber.BayTwo);
            this.HaveBay3 = allBays.Any(b => b.Number == BayNumber.BayThree);
            this.SingleBay = !this.HaveBay2 && !this.HaveBay3;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsBusy));

            this.saveCommand?.RaiseCanExecuteChanged();
            this.deleteCommand?.RaiseCanExecuteChanged();
            this.saveRotationClassCommand?.RaiseCanExecuteChanged();
        }

        private bool CanDelete()
        {
            return !this.IsBusy &&
                this.SelectedRotationClassSchedule != null &&
                this.SelectedRotationClassSchedule.Id != 0;
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.SelectedRotationClassSchedule != null;
        }

        private bool CanSaveRotationClass()
        {
            return !this.IsBusy && this.selectedBayNumber != BayNumber.None;
        }

        private async Task DeleteAsync()
        {
            //try
            //{
            //    this.IsBusy = true;

            // this.ClearNotifications();

            // //not implemented

            //    this.ShowNotification(Localized.Get("InstallationApp.RemoveSuccessful"));
            //}
            //catch (Exception ex)
            //{
            //    this.ShowNotification(ex);
            //}
            //finally
            //{
            //    await this.LoadSettings();

            //    this.IsBusy = false;
            //}
        }

        private async Task LoadSettings()
        {
            this.RotationClassSchedules.Clear();

            var rotationClassSheduleList = IEnumConvert(await this.machineIdentityWebService.GetAllRotationClassScheduleAsync());

            if (rotationClassSheduleList != null)
            {
                this.RotationClassSchedules.Add(rotationClassSheduleList.FirstOrDefault());
            }

            this.SelectedRotationClassSchedule = null;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                //var z = this.AutoCompactingSettings.Count;

                //if (this.RotationClassSchedules.Any(x => x.BeginTime == this.SelectedAutoCompactingSettings.BeginTime && x.Id != this.SelectedAutoCompactingSettings.Id))
                //{
                //    //this.ShowNotification(Localized.Get("InstallationApp.WrongDataSave"), Services.Models.NotificationSeverity.Error);
                //}
                //else
                //{
                await this.machineIdentityWebService.AddOrModifyRotationClassScheduleAsync(this.selectedRotationClassSchedule);

                this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
                //}
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                await this.LoadSettings();

                this.IsBusy = false;
            }
        }

        private async Task SaveRotationClassAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                await this.machineBaysWebService.SetRotationClassAsync(this.selectedBayNumber);

                this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                await this.LoadSettings();

                this.IsBusy = false;
            }
        }

        #endregion
    }
}
