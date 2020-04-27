using System;
using System.Collections.Generic;
using System.Linq;
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
    internal sealed class ParameterInverterViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private IEnumerable<InverterParameterSet> configuration;

        private bool isBusy;

        private InverterParameterSet selectedInverter;

        private DelegateCommand setInvertersParamertersCommand;

        private DelegateCommand<object> showInverterParamertersCommand;

        #endregion

        #region Constructors

        public ParameterInverterViewModel(IMachineDevicesWebService machineDevicesWebService)
            : base(PresentationMode.Installer)
        {
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public List<InverterParameterSet> Configuration => this.configuration?.ToList();

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.setInvertersParamertersCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public InverterParameterSet SelectedInverter
        {
            get => this.selectedInverter;
            set => this.SetProperty(ref this.selectedInverter, value);
        }

        public ICommand SetInvertersParamertersCommand =>
                   this.setInvertersParamertersCommand
               ??
               (this.setInvertersParamertersCommand = new DelegateCommand(
                async () => await this.SaveAllParametersAsync(), this.CanSave));

        public ICommand ShowInverterParamertersCommand =>
           this.showInverterParamertersCommand
       ??
       (this.showInverterParamertersCommand = new DelegateCommand<object>(this.ShowInverterParameters));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                this.configuration = await this.machineDevicesWebService.GetParametersAsync();
                this.RaisePropertyChanged(nameof(this.Configuration));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.isBusy;
        }

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                await this.machineDevicesWebService.ProgramAllInvertersAsync();

                this.ShowNotification(InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);

                this.RaisePropertyChanged(nameof(this.Configuration));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void ShowInverterParameters(object paramerter)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.PARAMETERSINVERTERDETAILS,
                data: paramerter,
                trackCurrentView: true); ;
        }

        #endregion
    }
}
