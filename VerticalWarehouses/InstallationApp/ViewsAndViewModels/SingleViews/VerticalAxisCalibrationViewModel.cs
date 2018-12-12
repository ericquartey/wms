using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.Navigation;
using Ferretto.VW.Utils.Source;
using System.Threading;
using Ferretto.VW.Utils.Source.Configuration;
using System.Threading.Tasks;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.InverterDriver.Source;
using System.Diagnostics;
using Ferretto.VW.ActionBlocks.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class VerticalAxisCalibrationViewModel : BindableBase
    {
        #region Fields

        private bool enableStartButton = true;
        private bool isStopButtonActive;
        private string lowerBound;
        private string noteString = Common.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
        private string offset;
        private bool originProcedureCanceled;
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

        public Boolean EnableStartButton { get => this.enableStartButton; set => this.SetProperty(ref this.enableStartButton, value); }

        public Boolean IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public String LowerBound { get => this.lowerBound; set { this.SetProperty(ref this.lowerBound, value); this.InputsCorrectionControlEventHandler(); } }

        public String NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public String Offset { get => this.offset; set { this.SetProperty(ref this.offset, value); this.InputsCorrectionControlEventHandler(); } }

        public String Resolution { get => this.resolution; set { this.SetProperty(ref this.resolution, value); this.InputsCorrectionControlEventHandler(); } }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(() => this.StopButtonMethod()));

        public String UpperBound { get => this.upperBound; set { this.SetProperty(ref this.upperBound, value); this.InputsCorrectionControlEventHandler(); } }

        #endregion Properties

        #region Methods

        private void Calibration(bool result)
        {
            this.EnableStartButton = true;
            this.IsStopButtonActive = false;
            this.NoteString = Common.Resources.InstallationApp.SetOriginVerticalAxisCompleted;
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.Offset, out var _offset) &&
                int.TryParse(this.Resolution, out var _resolution) &&
                int.TryParse(this.UpperBound, out var _upperBound))
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

        private async void ExecuteStartButtonCommand()
        {
            // Temporary the variables have a fixed value,
            // they will be variables when there'll be new functions
            if (ActionManager.CalibrateVerticalAxisInstance != null)
            {
                int m = 5;
                short ofs = 1;
                short vFast = 1;
                short vCreep = 1;

                this.EnableStartButton = false;
                this.IsStopButtonActive = true;
                await Task.Delay(2000);

                ActionManager.CalibrateVerticalAxisInstance.ThrowEndEvent += new CalibrateVerticalAixsEndedEventHandler(this.Calibration);
                this.NoteString = Common.Resources.InstallationApp.VerticalAxisCalibrating;
                ActionManager.CalibrateVerticalAxisInstance.SetVAxisOrigin(m, ofs, vFast, vCreep);
                this.NoteString = "Homing Done.";
            }
        }

        private void StopButtonMethod()
        {
            this.NoteString = Common.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
            this.originProcedureCanceled = true;
        }

        #endregion Methods
    }
}
