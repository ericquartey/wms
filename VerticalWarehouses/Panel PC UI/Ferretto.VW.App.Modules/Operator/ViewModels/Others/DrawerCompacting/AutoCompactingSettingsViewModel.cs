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
    internal sealed class AutoCompactingSettingsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineAutoCompactingSettingsWebService machineAutoCompactingSettingsWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ISessionService sessionService;

        private ObservableCollection<AutoCompactingSettings> autoCompactingSettings = new ObservableCollection<AutoCompactingSettings>();

        private DelegateCommand deleteCommand;

        private bool isBusy;

        private bool isInstaller;

        private bool isRotationClassEnabled;

        private DelegateCommand saveCommand;

        private AutoCompactingSettings selectedAutoCompactingSettings;

        #endregion

        #region Constructors

        public AutoCompactingSettingsViewModel(IMachineAutoCompactingSettingsWebService machineAutoCompactingSettingsWebService,
                                                IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Operator)
        {
            this.machineAutoCompactingSettingsWebService = machineAutoCompactingSettingsWebService ?? throw new ArgumentNullException(nameof(machineAutoCompactingSettingsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.AutoCompactingSettings = new ObservableCollection<AutoCompactingSettings>();

            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Properties

        public ObservableCollection<AutoCompactingSettings> AutoCompactingSettings
        {
            get => this.autoCompactingSettings;
            set => this.SetProperty(ref this.autoCompactingSettings, value, this.RaiseCanExecuteChanged);
        }

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

        public bool IsRotationClassEnabled
        {
            get => this.isRotationClassEnabled;
            set => this.SetProperty(ref this.isRotationClassEnabled, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
                                   this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
               async () => await this.SaveAsync(), this.CanSave));

        public AutoCompactingSettings SelectedAutoCompactingSettings
        {
            get => this.selectedAutoCompactingSettings;
            set => this.SetProperty(ref this.selectedAutoCompactingSettings, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.SelectedAutoCompactingSettings = null;
            this.AutoCompactingSettings.Clear();

            this.IsBusy = false;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.IsInstaller = this.sessionService.UserAccessLevel > UserAccessLevel.Movement;

            await this.LoadSettings();

            this.IsBusy = false;

            this.IsRotationClassEnabled = await this.machineIdentityWebService.GetIsRotationClassAsync();
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
                this.SelectedAutoCompactingSettings != null &&
                this.SelectedAutoCompactingSettings.Id != 0;
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.SelectedAutoCompactingSettings != null;
        }

        private async Task DeleteAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                await this.machineAutoCompactingSettingsWebService.RemoveAutoCompactingSettingsByIdAsync(this.SelectedAutoCompactingSettings.Id);

                this.ShowNotification(Localized.Get("InstallationApp.RemoveSuccessful"));
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

        private async Task LoadSettings()
        {
            this.AutoCompactingSettings = IEnumConvert(await this.machineAutoCompactingSettingsWebService.GetAllAutoCompactingSettingsAsync());

            if (this.sessionService.UserAccessLevel > UserAccessLevel.Movement && this.AutoCompactingSettings.Count < 3)
            {
                this.AutoCompactingSettings.Add(new AutoCompactingSettings());
            }

            this.SelectedAutoCompactingSettings = null;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                var z = this.AutoCompactingSettings.Count;

                if (this.AutoCompactingSettings.Any(x => x.BeginTime == this.SelectedAutoCompactingSettings.BeginTime && x.Id != this.SelectedAutoCompactingSettings.Id))
                {
                    this.ShowNotification(Localized.Get("InstallationApp.WrongDataSave"), Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    await this.machineAutoCompactingSettingsWebService.AddAutoCompactingSettingsAsync(this.SelectedAutoCompactingSettings);

                    this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
                }
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
