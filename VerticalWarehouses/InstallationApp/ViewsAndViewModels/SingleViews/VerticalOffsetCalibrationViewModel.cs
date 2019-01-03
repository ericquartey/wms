using System;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IViewModel, IVerticalOffsetCalibrationViewModel
    {
        #region Fields

        public IUnityContainer Container;
        private readonly float acc = 1;

        private readonly float dec = 1;

        private readonly short offset = 1;
        private readonly float vMax = 1;

        private readonly float w = 1;

        private string correctOffset;
        private ICommand correctOffsetButtonCommand;
        private string currentHeight;
        private int currentOffset;
        private ICommand exitFromViewCommand;
        private bool isCorrectOffsetButtonActive = false;
        private bool isSetPositionButtonActive = true;
        private bool isStepDownButtonActive = false;
        private bool isStepUpButtonActive = false;
        private string noteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;
        private short offsetValue;
        private PositioningDrawer positioningDrawerInstance;
        private string referenceCellHeight;
        private string referenceCellNumber;
        private ICommand setPositionButtonCommand;
        private ICommand stepDownButtonCommand;
        private ICommand stepUpButtonCommand;
        private string stepValue;

        #endregion Fields

        #region Constructors

        public VerticalOffsetCalibrationViewModel()
        {
            this.NoteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;
        }

        #endregion Constructors

        #region Properties

        public string CorrectOffset { get => this.correctOffset; set => this.SetProperty(ref this.correctOffset, value); }

        public ICommand CorrectOffsetButtonCommand => this.correctOffsetButtonCommand ?? (this.correctOffsetButtonCommand = new DelegateCommand(this.CorrectOffsetButtonCommandMethod));

        public string CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public int CurrentOffset { get => this.currentOffset; set => this.SetProperty(ref this.currentOffset, value); }

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public Boolean IsCorrectOffsetButtonActive { get => this.isCorrectOffsetButtonActive; set => this.SetProperty(ref this.isCorrectOffsetButtonActive, value); }

        public Boolean IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public Boolean IsStepDownButtonActive { get => this.isStepDownButtonActive; set => this.SetProperty(ref this.isStepDownButtonActive, value); }

        public Boolean IsStepUpButtonActive { get => this.isStepUpButtonActive; set => this.SetProperty(ref this.isStepUpButtonActive, value); }

        public String NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public short Offset => this.offsetValue;

        public string ReferenceCellHeight { get => this.referenceCellHeight; set => this.SetProperty(ref this.referenceCellHeight, value); }

        public string ReferenceCellNumber { get => this.referenceCellNumber; set => this.SetProperty(ref this.referenceCellNumber, value); }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(this.SetPositionButtonCommandMethod));

        public ICommand StepDownButtonCommand => this.stepDownButtonCommand ?? (this.stepDownButtonCommand = new DelegateCommand(this.StepDownButtonCommandMethod));

        public ICommand StepUpButtonCommand => this.stepUpButtonCommand ?? (this.stepUpButtonCommand = new DelegateCommand(this.StepUpButtonCommandMethod));

        public string StepValue { get => this.stepValue; set => this.SetProperty(ref this.stepValue, value); }

        #endregion Properties

        #region Methods

        public void CorrectOffsetButtonCommandMethod()
        {
            this.CorrectOffset = Convert.ToString(this.CurrentOffset);
            short.TryParse(this.CorrectOffset, out this.offsetValue);
        }

        public void ExitFromViewMethod()
        {
            // Unsubscribe methods
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.positioningDrawerInstance = (PositioningDrawer)this.Container.Resolve<IPositioningDrawer>();
        }

        public void PositioningDone(bool result)
        {
            var message = "";

            if (result)
            {
                this.IsSetPositionButtonActive = true;
                message = Common.Resources.InstallationApp.SetPosition;
            }
            else
            {
                message = "Set Positioning not Done";
            }

            this.NoteString = message;
            this.positioningDrawerInstance.Stop();

            this.IsStepUpButtonActive = true;
            this.IsStepDownButtonActive = true;
            this.IsCorrectOffsetButtonActive = true;
        }

        public void SetPositionButtonCommandMethod()
        {
            short.TryParse(this.ReferenceCellHeight, out var x);

            this.isSetPositionButtonActive = false;
            this.NoteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;

            this.positioningDrawerInstance.AbsoluteMovement = true;
            this.positioningDrawerInstance.MoveAlongVerticalAxisToPoint(x, this.vMax, this.acc, this.dec, this.w, this.offset);
        }

        public void StepDownButtonCommandMethod()
        {
            short.TryParse(this.StepValue, out var x);

            this.positioningDrawerInstance.AbsoluteMovement = false;
            this.positioningDrawerInstance.MoveAlongVerticalAxisToPoint(-x, this.vMax, this.acc, this.dec, this.w, this.offset);

            this.CurrentOffset -= x;
        }

        public void StepUpButtonCommandMethod()
        {
            short.TryParse(this.StepValue, out var x);

            this.positioningDrawerInstance.AbsoluteMovement = false;
            this.positioningDrawerInstance.MoveAlongVerticalAxisToPoint(x, this.vMax, this.acc, this.dec, this.w, this.offset);

            this.CurrentOffset += x;
        }

        public void SubscribeMethodToEvent()
        {
            if (this.positioningDrawerInstance != null)
            {
                this.positioningDrawerInstance.ThrowEndEvent += this.PositioningDone;
                this.positioningDrawerInstance.ThrowErrorEvent += this.PositioningError;
            }
        }

        public void UnSubscribeMethodFromEvent()
        {
            if (this.positioningDrawerInstance != null)
            {
                this.positioningDrawerInstance.ThrowErrorEvent -= this.PositioningError;
                this.positioningDrawerInstance.ThrowEndEvent -= this.PositioningDone;
            }
        }

        private void PositioningError(String error_Message)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
