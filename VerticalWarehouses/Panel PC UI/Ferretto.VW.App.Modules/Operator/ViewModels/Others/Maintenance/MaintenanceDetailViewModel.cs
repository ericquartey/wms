using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class MaintenanceDetailViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineServicingWebService machineServicingWebService;

        private DelegateCommand confirmServiceCommand;

        private List<Instruction> instructions;

        private string mainteinanceRequest;

        private ServicingInfo service;

        private int servicingInfoId = 0;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(IMachineServicingWebService machineServicingWebService)
            : base(PresentationMode.Operator)
        {
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
        }

        #endregion

        #region Properties

        public ICommand ConfirmServiceCommand =>
           this.confirmServiceCommand
           ??
           (this.confirmServiceCommand = new DelegateCommand(
              async () => await this.ConfirmServiceAsync(), this.CanConfirmService));

        public override EnableMask EnableMask => EnableMask.Any;

        public List<Instruction> Instructions
        {
            get => this.instructions;
            set => this.SetProperty(ref this.instructions, value);
        }

        public string MainteinanceRequest
        {
            get => this.mainteinanceRequest;
            set => this.SetProperty(ref this.mainteinanceRequest, value);
        }

        public ServicingInfo Service
        {
            get => this.service;
            set
            {
                this.SetProperty(ref this.service, value, this.RaiseCanExecuteChanged);

                if (this.service.ServiceStatus == MAS.AutomationService.Contracts.MachineServiceStatus.Valid)
                {
                    this.mainteinanceRequest = "Red";
                }

                if (this.service.ServiceStatus == MAS.AutomationService.Contracts.MachineServiceStatus.Expired)
                {
                    this.mainteinanceRequest = "Green";
                }

                if (this.service.ServiceStatus == MAS.AutomationService.Contracts.MachineServiceStatus.Expiring)
                {
                    this.mainteinanceRequest = "Orange";
                }

                if (this.service.ServiceStatus == MAS.AutomationService.Contracts.MachineServiceStatus.Completed)
                {
                    this.mainteinanceRequest = "White";
                }

                this.RaisePropertyChanged(nameof(this.MainteinanceRequest));
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.servicingInfoId = (int)this.Data;

            await this.GetServicingInfo();

            await this.GetGridElement();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.confirmServiceCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanConfirmService()
        {
            try
            {
                if (this.Instructions != null && (this.Instructions.Where(s => s.IsDone && s.IsToDo).Count() == this.Instructions.Count) && this.Service.ServiceStatus != MachineServiceStatus.Completed)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ConfirmServiceAsync()
        {
            try
            {
                await this.machineServicingWebService.ConfirmServiceAsync();
            }
            catch (Exception)
            {
            }
        }

        private async Task GetGridElement()
        {
            try
            {
                this.Instructions = this.Service.Instructions.ToList();
            }
            catch (Exception)
            {
            }
        }

        private async Task GetServicingInfo()
        {
            try
            {
                this.Service = await this.machineServicingWebService.GetByIdAsync(this.servicingInfoId);
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
