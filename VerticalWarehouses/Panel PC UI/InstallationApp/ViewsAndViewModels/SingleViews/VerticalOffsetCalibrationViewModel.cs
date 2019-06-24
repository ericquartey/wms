using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IVerticalOffsetCalibrationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private ICommand acceptOffsetButtonCommand;

        private IUnityContainer container;

        private string correctOffset;

        private ICommand correctOffsetButtonCommand;

        private string currentHeight;

        private decimal currentOffset;

        private ICommand exitFromViewCommand;

        private bool isAcceptOffsetButtonActive = true;

        private bool isCorrectOffsetButtonActive;

        private bool isSetPositionButtonActive = true;

        private bool isStepDownButtonActive = true;

        private bool isStepUpButtonActive = true;

        private string noteString = Ferretto.VW.Resources.InstallationApp.VerticalOffsetCalibration;

        private IOffsetCalibrationService offsetCalibrationService;

        private string referenceCellHeight;

        private string referenceCellNumber;

        private ICommand setPositionButtonCommand;

        private ICommand stepDownButtonCommand;

        private ICommand stepUpButtonCommand;

        private string stepValue;

        #endregion

        #region Constructors

        public VerticalOffsetCalibrationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NoteString = VW.Resources.InstallationApp.VerticalOffsetCalibration;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand AcceptOffsetButtonCommand => this.acceptOffsetButtonCommand ?? (
            this.acceptOffsetButtonCommand = new DelegateCommand(async () => await this.AcceptOffsetButtonCommandMethodAsync()));

        public string CorrectOffset { get => this.correctOffset; set => this.SetProperty(ref this.correctOffset, value); }

        public ICommand CorrectOffsetButtonCommand => this.correctOffsetButtonCommand ?? (
            this.correctOffsetButtonCommand = new DelegateCommand(async () => await this.CorrectOffsetButtonCommandMethodAsync()));

        public string CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public decimal CurrentOffset { get => this.currentOffset; set => this.SetProperty(ref this.currentOffset, value); }

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public bool IsAcceptOffsetButtonActive { get => this.isAcceptOffsetButtonActive; set => this.SetProperty(ref this.isAcceptOffsetButtonActive, value); }

        public bool IsCorrectOffsetButtonActive { get => this.isCorrectOffsetButtonActive; set => this.SetProperty(ref this.isCorrectOffsetButtonActive, value); }

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public bool IsStepDownButtonActive { get => this.isStepDownButtonActive; set => this.SetProperty(ref this.isStepDownButtonActive, value); }

        public bool IsStepUpButtonActive { get => this.isStepUpButtonActive; set => this.SetProperty(ref this.isStepUpButtonActive, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string ReferenceCellHeight { get => this.referenceCellHeight; set => this.SetProperty(ref this.referenceCellHeight, value); }

        public string ReferenceCellNumber { get => this.referenceCellNumber; set => this.SetProperty(ref this.referenceCellNumber, value); }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(this.SetPositionButtonCommandMethod));

        public ICommand StepDownButtonCommand => this.stepDownButtonCommand ?? (this.stepDownButtonCommand = new DelegateCommand(this.StepDownButtonCommandMethod));

        public ICommand StepUpButtonCommand => this.stepUpButtonCommand ?? (this.stepUpButtonCommand = new DelegateCommand(this.StepUpButtonCommandMethod));

        public string StepValue { get => this.stepValue; set => this.SetProperty(ref this.stepValue, value); }

        #endregion

        #region Methods

        public async Task AcceptOffsetButtonCommandMethodAsync()
        {
            this.CorrectOffset = "1,78";

            if (decimal.TryParse(this.CorrectOffset, out var decCorrectOffset))
            {
                var result = await this.offsetCalibrationService.SetOffsetParameterAsync(decCorrectOffset);

                if (result)
                {
                    this.IsAcceptOffsetButtonActive = false;
                    this.IsCorrectOffsetButtonActive = true;
                    this.IsStepDownButtonActive = false;
                    this.IsStepUpButtonActive = false;
                    this.CurrentOffset = decCorrectOffset;
                }
            }
        }

        public async Task CorrectOffsetButtonCommandMethodAsync()
        {
            var result = await this.offsetCalibrationService.ExecuteCompletedAsync();

            if (result)
            {
                this.IsCorrectOffsetButtonActive = false;
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.offsetCalibrationService = container.Resolve<IOffsetCalibrationService>();
            this.container = container;
        }

        public async Task OnEnterViewAsync()
        {
            // TODO implement missing feature
        }

        public void PositioningDone(bool result)
        {
            // TODO implement missing feature
        }

        public void SetPositionButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void StepDownButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void StepUpButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO implement missing feature
        }

        #endregion
    }
}
