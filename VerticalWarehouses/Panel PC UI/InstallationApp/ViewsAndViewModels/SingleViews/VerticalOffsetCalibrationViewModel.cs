using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IViewModel, IVerticalOffsetCalibrationViewModel
    {
        #region Fields

        public IUnityContainer Container;

        private string correctOffset;

        private ICommand correctOffsetButtonCommand;

        private string currentHeight;

        private int currentOffset;

        private IEventAggregator eventAggregator;

        private ICommand exitFromViewCommand;

        private bool isCorrectOffsetButtonActive;

        private bool isSetPositionButtonActive = true;

        private bool isStepDownButtonActive;

        private bool isStepUpButtonActive;

        private string noteString = Ferretto.VW.Resources.InstallationApp.VerticalOffsetCalibration;

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
            this.NoteString = Ferretto.VW.Resources.InstallationApp.VerticalOffsetCalibration;
        }

        #endregion

        #region Properties

        public string CorrectOffset { get => this.correctOffset; set => this.SetProperty(ref this.correctOffset, value); }

        public ICommand CorrectOffsetButtonCommand => this.correctOffsetButtonCommand ?? (this.correctOffsetButtonCommand = new DelegateCommand(this.CorrectOffsetButtonCommandMethod));

        public string CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public int CurrentOffset { get => this.currentOffset; set => this.SetProperty(ref this.currentOffset, value); }

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public bool IsCorrectOffsetButtonActive { get => this.isCorrectOffsetButtonActive; set => this.SetProperty(ref this.isCorrectOffsetButtonActive, value); }

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public bool IsStepDownButtonActive { get => this.isStepDownButtonActive; set => this.SetProperty(ref this.isStepDownButtonActive, value); }

        public bool IsStepUpButtonActive { get => this.isStepUpButtonActive; set => this.SetProperty(ref this.isStepUpButtonActive, value); }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string ReferenceCellHeight { get => this.referenceCellHeight; set => this.SetProperty(ref this.referenceCellHeight, value); }

        public string ReferenceCellNumber { get => this.referenceCellNumber; set => this.SetProperty(ref this.referenceCellNumber, value); }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(this.SetPositionButtonCommandMethod));

        public ICommand StepDownButtonCommand => this.stepDownButtonCommand ?? (this.stepDownButtonCommand = new DelegateCommand(this.StepDownButtonCommandMethod));

        public ICommand StepUpButtonCommand => this.stepUpButtonCommand ?? (this.stepUpButtonCommand = new DelegateCommand(this.StepUpButtonCommandMethod));

        public string StepValue { get => this.stepValue; set => this.SetProperty(ref this.stepValue, value); }

        #endregion

        #region Methods

        public void CorrectOffsetButtonCommandMethod()
        {
            // TODO implement missing feature
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
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

        public void SubscribeMethodToEvent()
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
