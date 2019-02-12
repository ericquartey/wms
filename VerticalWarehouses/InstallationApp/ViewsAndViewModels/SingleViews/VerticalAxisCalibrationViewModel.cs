using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Microsoft.Practices.Unity;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IViewModel, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        public IUnityContainer Container;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

        private string noteString = Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private string resolution;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel()
        {
            InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
        }

        #endregion

        #region Delegates

        public delegate void CheckCorrectnessOnPropertyChangedEventHandler();

        #endregion

        #region Events

        public event CheckCorrectnessOnPropertyChangedEventHandler InputsCorrectionControlEventHandler;

        #endregion

        #region Properties

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public string LowerBound { get => this.lowerBound; set { this.SetProperty(ref this.lowerBound, value); this.InputsCorrectionControlEventHandler(); } }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string Offset { get => this.offset; set { this.SetProperty(ref this.offset, value); this.InputsCorrectionControlEventHandler(); } }

        public string Resolution { get => this.resolution; set { this.SetProperty(ref this.resolution, value); this.InputsCorrectionControlEventHandler(); } }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(() => this.StopButtonMethod()));

        public string UpperBound { get => this.upperBound; set { this.SetProperty(ref this.upperBound, value); this.InputsCorrectionControlEventHandler(); } }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.Offset, out var _offset) &&
                int.TryParse(this.Resolution, out var _resolution) &&
                int.TryParse(this.UpperBound, out var _upperBound))
            { // TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                if (_lowerBound > 0 && _lowerBound < _upperBound && _upperBound > 0 && _resolution > 0 && _offset > 0)
                {
                    this.IsStartButtonActive = true;
                }
                else
                {
                    this.IsStartButtonActive = false;
                }
            }
            else
            {
                this.IsStartButtonActive = false;
            }
        }

        private void ExecuteStartButtonCommand()
        {
            // TODO implementa feature
        }

        private void StopButtonMethod()
        {
            this.NoteString = Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
        }

        #endregion
    }
}
