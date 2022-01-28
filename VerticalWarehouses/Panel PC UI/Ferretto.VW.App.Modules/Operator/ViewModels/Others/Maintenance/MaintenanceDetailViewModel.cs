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

        private List<Instruction> allInstructions = new List<Instruction>();

        private DelegateCommand bay1Command;

        private DelegateCommand bay2Command;

        private DelegateCommand bay3Command;

        private DelegateCommand cancelCommand;

        private DelegateCommand confirmInstructionCommand;

        private DelegateCommand confirmServiceCommand;

        private Senders currentGroup;

        private List<Instruction> currentGroupList = new List<Instruction>();

        private DelegateCommand horizontalAxisCommand;

        private List<Instruction> instructions = new List<Instruction>();

        private bool isActiveBay1;

        private bool isActiveBay2;

        private bool isActiveBay3;

        private bool isActiveHorizontalAxis;

        private bool isActiveMachine;

        private bool isActiveShutterBay1;

        private bool isActiveShutterBay2;

        private bool isActiveShutterBay3;

        private bool isActiveVerticalAxis;

        private bool isCompletedBay1;

        private bool isCompletedBay2;

        private bool isCompletedBay3;

        private bool isCompletedHorizontalAxis;

        private bool isCompletedMachine;

        private bool isCompletedShutterBay1;

        private bool isCompletedShutterBay2;

        private bool isCompletedShutterBay3;

        private bool isCompletedVerticalAxis;

        private bool isOperator;

        private bool isVisibleConfirmService;

        private int lastInstruction = 0;

        private DelegateCommand machineCommand;

        private string maintainerName;

        private string maintainerNote;

        private Instruction selectedInstruction;

        private ServicingInfo service;

        private int servicingInfoId = 0;

        private DelegateCommand setNoteCommand;

        private DelegateCommand showSetNoteCommand;

        private DelegateCommand shutterBay1Command;

        private DelegateCommand shutterBay2Command;

        private DelegateCommand shutterBay3Command;

        private DelegateCommand verticalAxisCommand;

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
            this.IsActiveVerticalAxis = true;
        }

        #endregion

        #region Enums

        private enum Senders
        {
            VerticalAxis = 1,

            HorizontalAxis = 2,

            Bay1 = 3,

            ShutterBay1 = 4,

            Bay2 = 5,

            ShutterBay2 = 6,

            Bay3 = 7,

            ShutterBay3 = 8,

            Machine = 9,
        }

        #endregion

        #region Properties

        public ICommand Bay1Command =>
            this.bay1Command ?? (this.bay1Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveBay1 = true; this.FilterTable(Senders.Bay1); }, this.CanFilterTable));

        public bool Bay1HasShutter => this.MachineService.Bays.SingleOrDefault(x => x.Number == BayNumber.BayOne)?.Shutter != null;

        public ICommand Bay2Command =>
                    this.bay2Command ?? (this.bay2Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveBay2 = true; this.FilterTable(Senders.Bay2); }, this.CanFilterTable));

        public bool Bay2HasShutter => this.MachineService.Bays.SingleOrDefault(x => x.Number == BayNumber.BayTwo)?.Shutter != null;

        public bool Bay2Present => this.MachineService.Bays.SingleOrDefault(x => x.Number == BayNumber.BayTwo) != null;

        public ICommand Bay3Command =>
                            this.bay3Command ?? (this.bay3Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveBay3 = true; this.FilterTable(Senders.Bay3); }, this.CanFilterTable));

        public bool Bay3HasShutter => this.MachineService.Bays.SingleOrDefault(x => x.Number == BayNumber.BayThree)?.Shutter != null;

        public bool Bay3Present => this.MachineService.Bays.SingleOrDefault(x => x.Number == BayNumber.BayThree) != null;

        public ICommand CancelCommand =>
            this.cancelCommand ?? (this.cancelCommand = new DelegateCommand(() => { this.ClosePopup(); }));

        public ICommand ConfirmInstructionCommand => this.confirmInstructionCommand ?? (this.confirmInstructionCommand = new DelegateCommand(async () => await this.ConfirmGroupAsync(), this.CanConfirmGroup));

        public ICommand ConfirmServiceCommand => this.confirmServiceCommand ?? (this.confirmServiceCommand = new DelegateCommand(async () => await this.ConfirmServiceAsync(), this.CanConfirmService));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand HorizontalAxisCommand =>
            this.horizontalAxisCommand ?? (this.horizontalAxisCommand = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveHorizontalAxis = true; this.FilterTable(Senders.HorizontalAxis); }, this.CanFilterTable));

        public List<Instruction> Instructions
        {
            get => this.instructions;
            set => this.SetProperty(ref this.instructions, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveBay1
        {
            get => this.isActiveBay1;
            set => this.SetProperty(ref this.isActiveBay1, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveBay2
        {
            get => this.isActiveBay2;
            set => this.SetProperty(ref this.isActiveBay2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveBay3
        {
            get => this.isActiveBay3;
            set => this.SetProperty(ref this.isActiveBay3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveHorizontalAxis
        {
            get => this.isActiveHorizontalAxis;
            set => this.SetProperty(ref this.isActiveHorizontalAxis, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveMachine
        {
            get => this.isActiveMachine;
            set => this.SetProperty(ref this.isActiveMachine, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveShutterBay1
        {
            get => this.isActiveShutterBay1;
            set => this.SetProperty(ref this.isActiveShutterBay1, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveShutterBay2
        {
            get => this.isActiveShutterBay2;
            set => this.SetProperty(ref this.isActiveShutterBay2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveShutterBay3
        {
            get => this.isActiveShutterBay3;
            set => this.SetProperty(ref this.isActiveShutterBay3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsActiveVerticalAxis
        {
            get => this.isActiveVerticalAxis;
            set => this.SetProperty(ref this.isActiveVerticalAxis, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedBay1
        {
            get => this.isCompletedBay1;
            set => this.SetProperty(ref this.isCompletedBay1, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedBay2
        {
            get => this.isCompletedBay2;
            set => this.SetProperty(ref this.isCompletedBay2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedBay3
        {
            get => this.isCompletedBay3;
            set => this.SetProperty(ref this.isCompletedBay3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedHorizontalAxis
        {
            get => this.isCompletedHorizontalAxis;
            set => this.SetProperty(ref this.isCompletedHorizontalAxis, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedMachine
        {
            get => this.isCompletedMachine;
            set => this.SetProperty(ref this.isCompletedMachine, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedShutterBay1
        {
            get => this.isCompletedShutterBay1;
            set => this.SetProperty(ref this.isCompletedShutterBay1, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedShutterBay2
        {
            get => this.isCompletedShutterBay2;
            set => this.SetProperty(ref this.isCompletedShutterBay2, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedShutterBay3
        {
            get => this.isCompletedShutterBay3;
            set => this.SetProperty(ref this.isCompletedShutterBay3, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCompletedVerticalAxis
        {
            get => this.isCompletedVerticalAxis;
            set => this.SetProperty(ref this.isCompletedVerticalAxis, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOperator
        {
            get => this.isOperator;
            set => this.SetProperty(ref this.isOperator, value);
        }

        public bool IsVisibleConfirmService
        {
            get => this.isVisibleConfirmService;
            set => this.SetProperty(ref this.isVisibleConfirmService, value);
        }

        public ICommand MachineCommand =>
            this.machineCommand ?? (this.machineCommand = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveMachine = true; this.FilterTable(Senders.Machine); }, this.CanFilterTable));

        public string MaintainerName
        {
            get => this.maintainerName;
            set => this.SetProperty(ref this.maintainerName, value, this.RaiseCanExecuteChanged);
        }

        public string MaintainerNote
        {
            get => this.maintainerNote;
            set => this.SetProperty(ref this.maintainerNote, value, this.RaiseCanExecuteChanged);
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

        public ICommand SetNoteCommand =>
            this.setNoteCommand ?? (this.setNoteCommand = new DelegateCommand(async () => this.SetNote()));

        public ICommand ShowSetNoteCommand =>
                    this.showSetNoteCommand ?? (this.showSetNoteCommand = new DelegateCommand(async () => this.ShowSetNote()));

        public ICommand ShutterBay1Command =>
            this.shutterBay1Command ?? (this.shutterBay1Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveShutterBay1 = true; this.FilterTable(Senders.ShutterBay1); }, this.CanFilterTable));

        public ICommand ShutterBay2Command =>
            this.shutterBay2Command ?? (this.shutterBay2Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveShutterBay2 = true; this.FilterTable(Senders.ShutterBay2); }, this.CanFilterTable));

        public ICommand ShutterBay3Command =>
            this.shutterBay3Command ?? (this.shutterBay3Command = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveShutterBay3 = true; this.FilterTable(Senders.ShutterBay3); }, this.CanFilterTable));

        public ICommand VerticalAxisCommand =>
            this.verticalAxisCommand ?? (this.verticalAxisCommand = new DelegateCommand(() => { this.IsActiveChange(); this.IsActiveVerticalAxis = true; this.FilterTable(Senders.VerticalAxis); }, this.CanFilterTable));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.currentGroup = Senders.VerticalAxis;

            this.ClosePopup();

            await base.OnAppearedAsync();

            this.isOperator = this.sessionService.UserAccessLevel == UserAccessLevel.Operator;

            this.RaisePropertyChanged(nameof(this.IsOperator));

            this.IsBackNavigationAllowed = true;

            this.servicingInfoId = (int)this.Data;

            this.lastInstruction = 0;

            await this.machineServicingWebService.RefreshDescriptionAsync(this.servicingInfoId);

            await this.GetServicingInfo();

            this.IsActiveChange();

            this.IsActiveVerticalAxis = true;

            if (!this.allInstructions.Any() && this.Service.ServiceStatus != MachineServiceStatus.Valid)
            {
                this.IsCompletedBay1 = true;
                this.IsCompletedBay2 = true;
                this.IsCompletedBay3 = true;
                this.IsCompletedShutterBay1 = true;
                this.IsCompletedShutterBay2 = true;
                this.IsCompletedShutterBay3 = true;
                this.IsCompletedVerticalAxis = true;
                this.IsCompletedHorizontalAxis = true;
                this.IsCompletedMachine = true;
            }
            else
            {
                this.IsCompletedBay1 = false;
                this.IsCompletedBay2 = false;
                this.IsCompletedBay3 = false;
                this.IsCompletedShutterBay1 = false;
                this.IsCompletedShutterBay2 = false;
                this.IsCompletedShutterBay3 = false;
                this.IsCompletedVerticalAxis = false;
                this.IsCompletedHorizontalAxis = false;
                this.IsCompletedMachine = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.showSetNoteCommand?.RaiseCanExecuteChanged();
            this.confirmInstructionCommand?.RaiseCanExecuteChanged();
            this.confirmServiceCommand?.RaiseCanExecuteChanged();

            this.bay1Command?.RaiseCanExecuteChanged();
            this.bay2Command?.RaiseCanExecuteChanged();
            this.bay3Command?.RaiseCanExecuteChanged();
            this.machineCommand?.RaiseCanExecuteChanged();
            this.shutterBay1Command?.RaiseCanExecuteChanged();
            this.shutterBay2Command?.RaiseCanExecuteChanged();
            this.shutterBay3Command?.RaiseCanExecuteChanged();
            this.verticalAxisCommand?.RaiseCanExecuteChanged();
            this.horizontalAxisCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanConfirmGroup()
        {
            try
            {
                return this.currentGroupList.Any(x => x.IsDone == false) && this.currentGroupList.Any() && (this.Service.ServiceStatus == MachineServiceStatus.Expired || this.Service.ServiceStatus == MachineServiceStatus.Expiring);
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
                var can = !this.allInstructions.Any(s => s.InstructionStatus == MachineServiceStatus.Expired || s.InstructionStatus == MachineServiceStatus.Expiring)
                    && (this.Service?.ServiceStatus == MachineServiceStatus.Expired || this.Service?.ServiceStatus == MachineServiceStatus.Expiring);
                return can;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool CanFilterTable()
        {
            return true;
        }

        private void CheckCompletedGroup()
        {
            try
            {
                switch (this.currentGroup)
                {
                    case Senders.VerticalAxis:
                        this.IsCompletedVerticalAxis = true;
                        break;

                    case Senders.HorizontalAxis:
                        this.IsCompletedHorizontalAxis = true;
                        break;

                    case Senders.Bay1:
                        this.IsCompletedBay1 = true;
                        break;

                    case Senders.ShutterBay1:
                        this.IsCompletedShutterBay1 = true;
                        break;

                    case Senders.Bay2:
                        this.IsCompletedBay2 = true;
                        break;

                    case Senders.ShutterBay2:
                        this.IsCompletedShutterBay2 = true;
                        break;

                    case Senders.Bay3:
                        this.IsCompletedBay3 = true;
                        break;

                    case Senders.ShutterBay3:
                        this.IsCompletedShutterBay3 = true;
                        break;

                    case Senders.Machine:
                        this.IsCompletedMachine = true;
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        private void ClosePopup()
        {
            this.IsVisibleConfirmService = false;
            this.MaintainerName = string.Empty;
            this.MaintainerNote = string.Empty;
        }

        private async Task ConfirmGroupAsync()
        {
            try
            {
                foreach (var instruction in this.currentGroupList)
                {
                    if (instruction.InstructionStatus == MachineServiceStatus.Expired || instruction.InstructionStatus == MachineServiceStatus.Expiring)
                    {
                        await this.machineServicingWebService.ConfirmInstructionAsync(instruction.Id);
                    }
                }

                this.CheckCompletedGroup();
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
                    await this.machineServicingWebService.ConfirmServiceAsync();

                    this.ClosePopup();

                    this.NavigationService.GoBack();
                }
            }
            catch (Exception)
            {
                this.dialogService.ShowMessage(Localized.Get("OperatorApp.Error"), "No Name", DialogType.Error, DialogButtons.OK);
            }
        }

        private void FilterTable(Senders sender)
        {
            this.currentGroup = sender;

            switch (sender)
            {
                case Senders.VerticalAxis:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.Axis == Axis.Vertical);
                    break;

                case Senders.HorizontalAxis:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.Axis == Axis.Horizontal);
                    break;

                case Senders.Bay1:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayOne && x.Definition.IsShutter == false);
                    break;

                case Senders.ShutterBay1:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayOne && x.Definition.IsShutter == true);
                    break;

                case Senders.Bay2:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayOne && x.Definition.IsShutter == false);
                    break;

                case Senders.ShutterBay2:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayTwo && x.Definition.IsShutter == true);
                    break;

                case Senders.Bay3:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayThree && x.Definition.IsShutter == false);
                    break;

                case Senders.ShutterBay3:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.BayNumber == BayNumber.BayThree && x.Definition.IsShutter == true);
                    break;

                case Senders.Machine:
                    this.currentGroupList = this.allInstructions.FindAll(x => x.Definition.IsSystem == true);
                    break;
            }

            List<Instruction> tempList = new List<Instruction>();

            foreach (var instruction in this.currentGroupList.OrderByDescending(o => o.Definition.Operation))
            {
                if (!tempList.Any(x => x.Definition.Description == instruction.Definition.Description && x.Definition.Operation >= instruction.Definition.Operation))
                {
                    tempList.Add(instruction);
                }
            }

            try
            {
                this.Instructions = tempList.OrderBy(o => o.Id).ToList();
                if (!this.Instructions.Exists(x => x.InstructionStatus == MachineServiceStatus.Expiring || x.InstructionStatus == MachineServiceStatus.Expired) && (this.Service?.ServiceStatus == MachineServiceStatus.Expired || this.Service?.ServiceStatus == MachineServiceStatus.Expiring))
                {
                    this.CheckCompletedGroup();
                }
            }
            catch (Exception)
            {
            }

            this.RaisePropertyChanged(nameof(this.Instructions));
        }

        private void GetGridElement()
        {
            try
            {
                if (this.Service.ServiceStatus == MachineServiceStatus.Completed)
                {
                    this.allInstructions = this.Service.Instructions.ToList();//.FindAll(x => x.InstructionStatus == MachineServiceStatus.Expiring || x.InstructionStatus == MachineServiceStatus.Expired || x.InstructionStatus == MachineServiceStatus.Completed);
                }
                else
                {
                    this.allInstructions = this.Service.Instructions.ToList().FindAll(x => x.InstructionStatus != MachineServiceStatus.Valid);//.FindAll(x => x.InstructionStatus == MachineServiceStatus.Expiring || x.InstructionStatus == MachineServiceStatus.Expired);
                }
                this.FilterTable(this.currentGroup);

                this.RaisePropertyChanged(nameof(this.Instructions));
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

        private void IsActiveChange()
        {
            this.IsActiveVerticalAxis = false;

            this.IsActiveHorizontalAxis = false;

            this.IsActiveBay1 = false;

            this.IsActiveShutterBay1 = false;

            this.IsActiveBay2 = false;

            this.IsActiveShutterBay2 = false;

            this.IsActiveBay3 = false;

            this.IsActiveShutterBay3 = false;

            this.IsActiveMachine = false;
        }

        private async void SetNote()
        {
            await this.machineServicingWebService.SetNoteAsync(this.maintainerName, this.maintainerNote, this.Service.Id);
            await this.GetServicingInfo();
            this.ClosePopup();
        }

        private void ShowSetNote()
        {
            this.IsVisibleConfirmService = true;
        }

        #endregion
    }
}
