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

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class MaintenanceViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand confirmServiceCommand;

        private DelegateCommand fixServicingInfoCommand;

        private bool canFix;

        private bool isBay2;

        private bool isBay2Operator;

        private bool isBay3;

        private bool isBay3Operator;

        private bool isConfirmServiceVisible;

        private bool isOperatorOrMovement;

        private string machineModel;

        private string machineSerial;

        private DelegateCommand maintenanceDetailButtonCommand;

        private ServicingInfo selectedServicingInfo;

        private IEnumerable<ServicingInfo> servicingInfo;

        private MachineStatistics statistics;

        #endregion

        #region Constructors

        public MaintenanceViewModel(
            ISessionService sessionService,
            IMachineServicingWebService machineServicingWebService,
            IMachineBaysWebService machineBaysWebService)
            : base(PresentationMode.Operator)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
            this.machineBaysWebService = machineBaysWebService ?? throw new System.ArgumentNullException(nameof(machineBaysWebService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmServiceCommand =>
           this.confirmServiceCommand
           ??
           (this.confirmServiceCommand = new DelegateCommand(
              async () => await this.ConfirmServiceAsync(), this.CanConfirmService));

        public ICommand FixServicingInfoCommand =>
           this.fixServicingInfoCommand
           ??
           (this.fixServicingInfoCommand = new DelegateCommand(
              async () => await this.FixServicingInfo(), this.CanFixServicingInfo));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool CanFix
        {
            get => this.canFix;
            set => this.SetProperty(ref this.canFix, value);
        }

        public bool IsBay2
        {
            get => this.isBay2;
            set => this.SetProperty(ref this.isBay2, value);
        }

        public bool IsBay2Operator
        {
            get => this.isBay2Operator;
            set => this.SetProperty(ref this.isBay2Operator, value);
        }

        public bool IsBay3
        {
            get => this.isBay3;
            set => this.SetProperty(ref this.isBay3, value);
        }

        public bool IsBay3Operator
        {
            get => this.isBay3Operator;
            set => this.SetProperty(ref this.isBay3Operator, value);
        }

        public bool IsConfirmServiceVisible
        {
            get => this.isConfirmServiceVisible;
            set => this.SetProperty(ref this.isConfirmServiceVisible, value);
        }

        public bool IsOperatorOrMovement
        {
            get => this.isOperatorOrMovement;
            set => this.SetProperty(ref this.isOperatorOrMovement, value);
        }

        public string MachineModel
        {
            get => this.machineModel;
            set => this.SetProperty(ref this.machineModel, value);
        }

        public string MachineSerial
        {
            get => this.machineSerial;
            set => this.SetProperty(ref this.machineSerial, value);
        }

        public ICommand MaintenanceDetailButtonCommand =>
                    this.maintenanceDetailButtonCommand
            ??
            (this.maintenanceDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        public ServicingInfo SelectedServicingInfo
        {
            get => this.selectedServicingInfo;
            set
            {
                this.SetProperty(ref this.selectedServicingInfo, value, this.RaiseCanExecuteChanged);
                this.GetStatistics();
            }
        }

        public IEnumerable<ServicingInfo> ServicingInfo
        {
            get => this.servicingInfo;
            set => this.SetProperty(ref this.servicingInfo, value);
        }

        public MachineStatistics Statistics
        {
            get => this.statistics;
            set => this.SetProperty(ref this.statistics, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.Statistics = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.CanFix = this.CanFixServicingInfo();
            this.RaisePropertyChanged(nameof(this.CanFix));

            this.IsOperatorOrMovement = this.sessionService.UserAccessLevel <= UserAccessLevel.Movement;
            var bays = await this.machineBaysWebService.GetAllAsync();

            this.IsBay2 = bays.Any(b => b.Number == BayNumber.BayTwo);
            this.IsBay3 = bays.Any(b => b.Number == BayNumber.BayThree);

            this.IsBay2Operator = this.IsBay2 && !this.IsOperatorOrMovement;
            this.IsBay3Operator = this.IsBay3 && !this.IsOperatorOrMovement;

            await this.machineServicingWebService.UpdateServiceStatusAsync();

            this.IsBackNavigationAllowed = true;

            this.MachineSerial = this.sessionService.MachineIdentity.SerialNumber;

            this.MachineModel = this.sessionService.MachineIdentity.ModelName;

            this.RaisePropertyChanged(nameof(this.MachineSerial));

            this.RaisePropertyChanged(nameof(this.MachineModel));

            var lst = await this.machineServicingWebService.GetAllAsync();

            this.ServicingInfo = lst.ToList();

            this.RaisePropertyChanged(nameof(this.ServicingInfo));

            this.SelectedServicingInfo = this.servicingInfo.ElementAtOrDefault(0);

            this.RaisePropertyChanged(nameof(this.SelectedServicingInfo));
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.maintenanceDetailButtonCommand?.RaiseCanExecuteChanged();
            this.fixServicingInfoCommand?.RaiseCanExecuteChanged();

            this.CanFix = this.CanFixServicingInfo();
            this.RaisePropertyChanged(nameof(this.CanFix));

            base.RaiseCanExecuteChanged();
        }

        private bool CanConfirmService() => true;

        private bool CanDetailCommand() => this.selectedServicingInfo != null && !this.IsOperatorOrMovement;

        private async Task ConfirmServiceAsync()
        {
            try
            {
                await this.machineServicingWebService.ConfirmServiceAsync();
            }
            catch
            {
                // do nothing
            }
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.Maintenance.DETAIL,
                    this.selectedServicingInfo.Id,
                    trackCurrentView: true);
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

        private async Task FixServicingInfo()
        {
            try
            {
                await this.machineServicingWebService.FixServicingInfoAsync();

                await this.OnAppearedAsync();

                this.Logger.Debug("Force Fix Maintenance");

                this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private bool CanFixServicingInfo()
        {
            var lastServicing = this.ServicingInfo?.Reverse().FirstOrDefault();

            if (lastServicing == null)
            {
                return false;
            }

            return this.sessionService.UserAccessLevel > UserAccessLevel.Installer
                   &&
                   lastServicing.ServiceStatus == MachineServiceStatus.Expired
                   &&
                   DateTime.Now <= lastServicing.NextServiceDate;
        }

        private async void GetStatistics()
        {
            this.IsWaitingForResponse = true;
            try
            {
                if (this.selectedServicingInfo != null)
                {
                    this.Statistics = this.SelectedServicingInfo.MachineStatistics;
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
        }

        #endregion
    }
}
