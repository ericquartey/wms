using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.InstallationApp.Views;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewModels
{
    internal class VerticalAxisCalibrationViewModel : BindableBase
    {
        #region Fields

        private bool enableNextButton;
        private bool enableStartButton;
        private bool enableStopButton;
        private string lowerBound;
        private ICommand nextButtonCommand;
        private string offset;
        private string resolution;
        private ICommand startButtonCommand;
        private ICommand stopButtonCommand;
        private string upperBound;

        #endregion Fields

        #region Constructors

        public VerticalAxisCalibrationViewModel()
        {
            InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
        }

        #endregion Constructors

        #region Delegates

        public delegate void CheckCorrectnessOnPropertyChangedEventHandler();

        #endregion Delegates

        #region Events

        public event CheckCorrectnessOnPropertyChangedEventHandler InputsCorrectionControlEventHandler;

        #endregion Events

        #region Properties

        public Boolean EnableNextButton { get => this.enableNextButton; set => this.SetProperty(ref this.enableNextButton, value); }
        public Boolean EnableStartButton { get => this.enableStartButton; set => this.SetProperty(ref this.enableStartButton, value); }
        public Boolean EnableStopButton { get => this.enableStopButton; set => this.SetProperty(ref this.enableStopButton, value); }
        public String LowerBound { get => this.lowerBound; set { this.SetProperty(ref this.lowerBound, value); this.InputsCorrectionControlEventHandler(); } }
        public ICommand NextButtonCommand => this.nextButtonCommand ?? (this.nextButtonCommand = new DelegateCommand(this.ExecuteNextButtonCommand));
        public String Offset { get => this.offset; set { this.SetProperty(ref this.offset, value); this.InputsCorrectionControlEventHandler(); } }
        public String Resolution { get => this.resolution; set { this.SetProperty(ref this.resolution, value); this.InputsCorrectionControlEventHandler(); } }
        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));
        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(this.ExecuteStopButtonCommand));
        public String UpperBound { get => this.upperBound; set { this.SetProperty(ref this.upperBound, value); this.InputsCorrectionControlEventHandler(); } }

        #endregion Properties

        #region Methods

        private void CheckInputsCorrectness()
        {
            int _lowerBound, _offset, _resolution, _upperBound;
            if (int.TryParse(this.LowerBound, out _lowerBound) &&
                int.TryParse(this.Offset, out _offset) &&
                int.TryParse(this.Resolution, out _resolution) &&
                int.TryParse(this.UpperBound, out _upperBound))
            {//TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                if (_lowerBound > 0 && _lowerBound < _upperBound && _upperBound > 0 && _resolution > 0 && _offset > 0)
                {
                    this.EnableStartButton = true;
                }
                else
                {
                    this.EnableStartButton = false;
                }
            }
            else
            {
                this.EnableStartButton = false;
            }
        }

        private void ExecuteNextButtonCommand()
        {
            //TODO: implement next button functionality
        }

        private void ExecuteStartButtonCommand()
        {
            //TODO: implement start button functionality
        }

        private void ExecuteStopButtonCommand()
        {
            //TODO: implement stop button functionality
        }

        #endregion Methods
    }
}
