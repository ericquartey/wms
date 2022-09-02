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

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand deleteCommand;

        private bool isBusy;

        private bool isInstaller;

        private ObservableCollection<RotationClassSchedule> rotationClassSchedules = new ObservableCollection<RotationClassSchedule>();

        private DelegateCommand saveCommand;

        private RotationClassSchedule selectedRotationClassSchedule;

        #endregion

        #region Constructors

        public DaysCountViewModel(IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Operator)
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Properties

        public ICommand DeleteCommand =>
                                           this.deleteCommand
           ??
           (this.deleteCommand = new DelegateCommand(
               async () => await this.DeleteAsync(), this.CanDelete));

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

        public RotationClassSchedule SelectedRotationClassSchedule
        {
            get => this.selectedRotationClassSchedule;
            set => this.SetProperty(ref this.selectedRotationClassSchedule, value, this.RaiseCanExecuteChanged);
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
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsBusy));

            this.saveCommand?.RaiseCanExecuteChanged();
            this.deleteCommand?.RaiseCanExecuteChanged();
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

        private async Task DeleteAsync()
        {
            //try
            //{
            //    this.IsBusy = true;

            //    this.ClearNotifications();

            //    //not implemented

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

        #endregion
    }
}
