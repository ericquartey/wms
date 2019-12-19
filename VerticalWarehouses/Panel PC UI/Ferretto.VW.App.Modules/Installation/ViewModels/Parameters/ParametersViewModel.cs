using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParametersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private VertimagConfiguration configuration;

        private DelegateCommand goToExport;

        private DelegateCommand goToImport;

        private bool isBusy;

        private DelegateCommand saveCommand;

        #endregion

        #region Constructors

        public ParametersViewModel(IMachineConfigurationWebService machineConfigurationWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public VertimagConfiguration Configuration => this.configuration;

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand GoToExport => this.goToExport
            ??
            (this.goToExport = new DelegateCommand(
                this.ShowExport, this.CanShowExport));

        public ICommand GoToImport => this.goToImport
                    ??
            (this.goToImport = new DelegateCommand(
                this.ShowImport, this.CanShowImport));

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    ((DelegateCommand)this.goToExport).RaiseCanExecuteChanged();
                    ((DelegateCommand)this.goToImport).RaiseCanExecuteChanged();
                    ((DelegateCommand)this.saveCommand).RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand SaveCommand =>
           this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
            async () => await this.SaveAsync(), this.CanSave));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;
                this.IsBackNavigationAllowed = true;
                this.configuration = await this.machineConfigurationWebService.GetAsync();
                this.RaisePropertyChanged(nameof(this.Configuration));
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

        private bool CanShowExport()
        {
            return !this.IsBusy;
        }

        private bool CanShowImport()
        {
            return !this.IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                await this.machineConfigurationWebService.SetAsync(this.configuration);

                this.ShowNotification(Resources.InstallationApp.SaveSuccessful);

                this.IsBusy = false;
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

        private void ShowExport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Parameters.PARAMETERSEXPORT,
                this.Configuration,
                trackCurrentView: true);
        }

        private void ShowImport()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Parameters.PARAMETERSIMPORTSTEP1,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
