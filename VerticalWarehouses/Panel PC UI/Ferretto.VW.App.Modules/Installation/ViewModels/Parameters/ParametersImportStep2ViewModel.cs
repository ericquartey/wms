using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class ParametersImportStep2ViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private VertimagConfiguration configuration;

        private DelegateCommand confirmSaveCommand;

        private bool isBusy;

        #endregion

        #region Constructors

        public ParametersImportStep2ViewModel(IMachineConfigurationWebService machineConfigurationWebService)
                : base(PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public VertimagConfiguration Configuration => this.configuration;

        public ICommand ConfirmSaveCommand =>
                    this.confirmSaveCommand
                   ??
                   (this.confirmSaveCommand = new DelegateCommand(
                       async () => await this.SaveAsync(), this.CanSave));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.RaisePropertyChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;

                if (this.Data is VertimagConfiguration vertimagConfiguration)
                {
                    this.configuration = vertimagConfiguration;
                    this.RaisePropertyChanged(nameof(this.Configuration));
                }
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

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                var messageBoxResult = dialogService.ShowMessage(InstallationApp.SureToApplyConfiguration, InstallationApp.ConfirmRestore, DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult != DialogResult.Yes)
                {
                    return;
                }

                this.ClearNotifications();

                await this.machineConfigurationWebService.SetAsync(this.configuration);

                this.ShowNotification(InstallationApp.RestoreSuccessful);
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
