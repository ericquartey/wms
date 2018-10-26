using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Ferretto.VW.Navigation;
using Ferretto.VW.Utils.Source;
using System.Threading;
using Ferretto.VW.Utils.Source.Configuration;
using System.Threading.Tasks;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class VerticalAxisCalibrationViewModel : BindableBase
    {
        #region Fields

        private bool enableStartButton = true;
        private string lowerBound;
        private string offset;
        private string resolution;
        private ICommand startButtonCommand;
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
        public String LowerBound { get => this.lowerBound; set { this.SetProperty(ref this.lowerBound, value); this.InputsCorrectionControlEventHandler(); } }
        public String Offset { get => this.offset; set { this.SetProperty(ref this.offset, value); this.InputsCorrectionControlEventHandler(); } }
        public String Resolution { get => this.resolution; set { this.SetProperty(ref this.resolution, value); this.InputsCorrectionControlEventHandler(); } }
        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));
        public String UpperBound { get => this.upperBound; set { this.SetProperty(ref this.upperBound, value); this.InputsCorrectionControlEventHandler(); } }

        #endregion Properties

        #region Methods

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
            //TODO: implement start button functionality
            //Temporary stuff to check DataManager behaviour
            this.EnableStartButton = false;
            await Task.Delay(2000);
            this.EnableStartButton = true;
            var ii = new Installation_Info();
            ii.Set_Y_Resolution = true;
            DataMngr.CurrentData.InstallationInfo = ii;
        }

        #endregion Methods
    }
}
