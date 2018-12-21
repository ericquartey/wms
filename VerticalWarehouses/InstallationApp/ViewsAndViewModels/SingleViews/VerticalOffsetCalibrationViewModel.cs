using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver.Source;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalOffsetCalibrationViewModel : BindableBase, IViewModel
    {
        #region Fields
        private string referenceCellNumber;
        private string referenceCellHeight;
        private string currentHeight;
        private string stepValue;
        private int currentOffset;
        private string correctOffset;
        private short offsetValue;
        private string noteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;
        private ICommand setPositionButtonCommand;
        private ICommand stepUpButtonCommand;
        private ICommand stepDownButtonCommand;
        private ICommand correctOffsetButtonCommand;
        private bool isStepUpButtonActive = false;
        private bool isStepDownButtonActive = false;
        private bool isCorrectOffsetButtonActive = false;
        private bool isSetPositionButtonActive = true;

        private readonly float vMax = 1;                //Temporary constant value.
        private readonly float acc = 1;                 //Temporary constant value.
        private readonly float dec = 1;                 //Temporary constant value.
        private readonly float w = 1;                   //Temporary constant value.
        private readonly short offset = 1;              //Temporary constant value.

        #endregion Fields

        #region Properties
        public string ReferenceCellNumber { get => this.referenceCellNumber; set => this.SetProperty(ref this.referenceCellNumber, value); }
        public string ReferenceCellHeight { get => this.referenceCellHeight; set => this.SetProperty(ref this.referenceCellHeight, value); }
        public string CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }
        public string StepValue { get => this.stepValue; set => this.SetProperty(ref this.stepValue, value); }
        public int CurrentOffset { get => this.currentOffset; set => this.SetProperty(ref this.currentOffset, value); }
        public string CorrectOffset { get => this.correctOffset; set => this.SetProperty(ref this.correctOffset, value); }
        public String NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }
        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(this.SetPositionButtonCommandMethod));
        public ICommand StepUpButtonCommand => this.stepUpButtonCommand ?? (this.stepUpButtonCommand = new DelegateCommand(this.StepUpButtonCommandMethod));
        public ICommand StepDownButtonCommand => this.stepDownButtonCommand ?? (this.stepDownButtonCommand = new DelegateCommand(this.StepDownButtonCommandMethod));
        public ICommand CorrectOffsetButtonCommand => this.correctOffsetButtonCommand ?? (this.correctOffsetButtonCommand = new DelegateCommand(this.CorrectOffsetButtonCommandMethod));
        public short Offset => this.offsetValue;
        public Boolean IsStepUpButtonActive { get => this.isStepUpButtonActive; set => this.SetProperty(ref this.isStepUpButtonActive, value); }
        public Boolean IsStepDownButtonActive { get => this.isStepDownButtonActive; set => this.SetProperty(ref this.isStepDownButtonActive, value); }
        public Boolean IsCorrectOffsetButtonActive { get => this.isCorrectOffsetButtonActive; set => this.SetProperty(ref this.isCorrectOffsetButtonActive, value); }
        public Boolean IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        #endregion Properties

        #region Constructors

        public VerticalOffsetCalibrationViewModel()
        {
            this.NoteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;
        }

        #endregion Constructors

        #region Methods
        
        public void ExitFromViewMethod()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeMethodToEvent()
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new System.NotImplementedException();
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
            ActionManager.PositioningDrawerInstance.Stop();

            this.IsStepUpButtonActive = true;
            this.IsStepDownButtonActive = true;
            this.IsCorrectOffsetButtonActive = true;
        }

        public void SetPositionButtonCommandMethod()
        {
            short.TryParse(this.ReferenceCellHeight, out var x);

            this.isSetPositionButtonActive = false;
            this.NoteString = Common.Resources.InstallationApp.VerticalOffsetCalibration;

            ActionManager.PositioningDrawerInstance.AbsoluteMovement = true;
            ActionManager.PositioningDrawerInstance.ThrowEndEvent += new PositioningDrawerEndEventHandler(this.PositioningDone);
            ActionManager.PositioningDrawerInstance.MoveAlongVerticalAxisToPoint(x, this.vMax, this.acc, this.dec, this.w, this.offset);
        }

        public void StepUpButtonCommandMethod()
        {
            short.TryParse(this.StepValue, out var x);

            ActionManager.PositioningDrawerInstance.AbsoluteMovement = false;
            ActionManager.PositioningDrawerInstance.MoveAlongVerticalAxisToPoint(x, this.vMax, this.acc, this.dec, this.w, this.offset);

            this.CurrentOffset = this.CurrentOffset + x;
        }

        public void StepDownButtonCommandMethod()
        {
            short.TryParse(this.StepValue, out var x);

            ActionManager.PositioningDrawerInstance.AbsoluteMovement = false;
            ActionManager.PositioningDrawerInstance.MoveAlongVerticalAxisToPoint((short)(-1 * x), this.vMax, this.acc, this.dec, this.w, this.offset);

            this.CurrentOffset = this.CurrentOffset + (short)(-1 * x);
        }

        public void CorrectOffsetButtonCommandMethod()
        {
            this.CorrectOffset = Convert.ToString(this.CurrentOffset);
            short.TryParse(this.CorrectOffset, out this.offsetValue);
        }

        #endregion Methods
    }
}
