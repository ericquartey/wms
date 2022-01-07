using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
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
    public class MaintenanceDetailViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineServicingWebService machineServicingWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand confirmInstructionCommand;

        private DelegateCommand confirmServiceCommand;

        private DelegateCommand executeRowCommand;

        private List<Instruction> instructions = new List<Instruction>();

        private bool isOperator;

        private int lastInstruction = 0;

        private Instruction selectedInstruction;

        private ServicingInfo service;

        private int servicingInfoId = 0;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(
            IMachineServicingWebService machineServicingWebService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.machineServicingWebService = machineServicingWebService ?? throw new ArgumentNullException(nameof(machineServicingWebService));
            this.sessionService = ServiceLocator.Current.GetInstance<ISessionService>();
        }

        #endregion

        #region Properties

        public ICommand ConfirmInstructionCommand =>
           this.confirmInstructionCommand
           ??
           (this.confirmInstructionCommand = new DelegateCommand(
              async () => await this.ConfirmInstructionAsync(), this.CanConfirmInstruction));

        public ICommand ConfirmServiceCommand =>
                   this.confirmServiceCommand
           ??
           (this.confirmServiceCommand = new DelegateCommand(
              async () => await this.ConfirmServiceAsync(), this.CanConfirmService));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ExecuteRowCommand =>
                           this.executeRowCommand
           ??
           (this.executeRowCommand = new DelegateCommand(
              async () => await this.ExecuteRowAsync(), this.CanExecuteRow));

        public List<Instruction> Instructions => new List<Instruction>(this.instructions);

        public bool IsOperator
        {
            get => this.isOperator;
            set => this.SetProperty(ref this.isOperator, value);
        }

        public Instruction SelectedInstruction
        {
            get => this.selectedInstruction;
            set => this.SetProperty(ref this.selectedInstruction, value, this.RaiseCanExecuteChanged);
        }

        public ServicingInfo Service
        {
            get => this.service;
            set => this.SetProperty(ref this.service, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.isOperator = this.sessionService.UserAccessLevel == UserAccessLevel.Operator;

            this.RaisePropertyChanged(nameof(this.IsOperator));

            this.IsBackNavigationAllowed = true;

            this.servicingInfoId = (int)this.Data;

            this.lastInstruction = 0;

            await this.machineServicingWebService.RefreshDescriptionAsync(this.servicingInfoId);

            await this.GetServicingInfo();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.confirmServiceCommand?.RaiseCanExecuteChanged();
            this.confirmInstructionCommand?.RaiseCanExecuteChanged();
            this.executeRowCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private static string SetStatusColor(MachineServiceStatus status)
        {
            switch (status)
            {
                case MachineServiceStatus.Valid:
                    return "Green";

                case MachineServiceStatus.Expired:
                    return "Red";

                case MachineServiceStatus.Expiring:
                    return "Orange";

                case MachineServiceStatus.Completed:
                    return "White";

                default:
                    return "Transparent";
            }
        }

        private bool CanConfirmInstruction()
        {
            try
            {
                return this.SelectedInstruction != null &&
                    !this.SelectedInstruction.IsDone &&
                    this.SelectedInstruction.IsToDo &&
                    this.SelectedInstruction.InstructionStatus != MachineServiceStatus.Completed;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanConfirmService()
        {
            try
            {
                var can = this.Instructions.Any(s => s.IsToDo && s.IsDone)
                    && this.Service.ServiceStatus != MachineServiceStatus.Completed
                    && (DateTime.Now - this.Service.LastServiceDate.Value).Days > 0;
                return can;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanExecuteRow()
        {
            try
            {
                return this.SelectedInstruction != null &&
                    !this.SelectedInstruction.IsToDo;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task ConfirmInstructionAsync()
        {
            try
            {
                this.lastInstruction = this.selectedInstruction.Id;
                await this.machineServicingWebService.ConfirmInstructionAsync(this.SelectedInstruction.Id);
            }
            catch (Exception)
            {
            }
            finally
            {
                await this.GetServicingInfo();
            }
        }

        private async Task ConfirmServiceAsync()
        {
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("OperatorApp.ConfirmServiceMessage"), Localized.Get("OperatorApp.ConfirmService"), DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.lastInstruction = this.selectedInstruction.Id;
                    await this.machineServicingWebService.ConfirmServiceAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task ExecuteRowAsync()
        {
            try
            {
                this.lastInstruction = this.selectedInstruction.Id;
                await this.machineServicingWebService.SetIsToDoAsync(this.SelectedInstruction.Id);
            }
            catch (Exception)
            {
            }
            finally
            {
                await this.GetServicingInfo();
            }
        }

        private void GetGridElement()
        {
            try
            {
                this.instructions = this.Service.Instructions.ToList();
                this.RaisePropertyChanged(nameof(this.Instructions));

                if (this.instructions.Any())
                {
                    if (this.lastInstruction == this.instructions.FirstOrDefault().Id || this.lastInstruction == 0)
                    {
                        this.SelectedInstruction = this.instructions.ElementAtOrDefault(this.lastInstruction);
                        this.RaisePropertyChanged(nameof(this.SelectedInstruction));
                    }
                    else
                    {
                        this.SelectedInstruction = this.instructions.FirstOrDefault(s => s.Id == this.lastInstruction);
                        this.RaisePropertyChanged(nameof(this.SelectedInstruction));
                    }
                }
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

                this.GetGridElement();
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
