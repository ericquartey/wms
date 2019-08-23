using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CellsHeightCheckStep1ViewModel : BaseCellsHeightCheckViewModel
    {
        #region Fields

        private readonly IMachineElevatorService machineElevatorService;

        private int? inputCellId;

        private bool isElevatorMoving;

        private bool isWaitingForResponse;

        private DelegateCommand moveToCellHeightCommand;

        #endregion

        #region Constructors

        public CellsHeightCheckStep1ViewModel(
            IMachineCellsService machineCellsService,
            IMachineElevatorService machineElevatorService)
            : base(machineCellsService, machineElevatorService)
        {
        }

        #endregion

        #region Properties

        public int? InputCellId
        {
            get => this.inputCellId;
            set
            {
                if (this.SetProperty(ref this.inputCellId, value)
                    &&
                    this.Cells != null)
                {
                    this.SelectedCell = value == null
                        ? null
                        : this.Cells.SingleOrDefault(c => c.Id == value);
                }
            }
        }

        public bool IsElevatorMoving
        {
            get => this.isElevatorMoving;
            set
            {
                if (this.SetProperty(ref this.isElevatorMoving, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set
            {
                if (this.SetProperty(ref this.isWaitingForResponse, value))
                {
                    this.RefreshCanExecuteCommands();
                }
            }
        }

        public ICommand MoveToCellHeightCommand =>
            this.moveToCellHeightCommand
            ??
            (this.moveToCellHeightCommand = new DelegateCommand(
                async () => await this.ExecuteMoveToCellHeightCommand(),
                this.CanExecuteMoveToCellHeightCommand));

        #endregion

        #region Methods

        private bool CanExecuteMoveToCellHeightCommand()
        {
            return this.SelectedCell != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsElevatorMoving;
        }

        private async Task ExecuteMoveToCellHeightCommand()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineElevatorService.MoveToVerticalPositionAsync(this.SelectedCell.Coord);

                this.IsElevatorMoving = true;
            }
            catch (System.Exception ex)
            {
                this.IsElevatorMoving = true;

                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = true;
            }
        }

        protected override void RefreshCanExecuteCommands()
        {
            this.moveToCellHeightCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
