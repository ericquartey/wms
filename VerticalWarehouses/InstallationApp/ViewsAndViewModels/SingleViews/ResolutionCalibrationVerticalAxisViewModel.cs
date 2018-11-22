using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class ResolutionCalibrationVerticalAxisViewModel : BindableBase
    {
        #region Fields

        private readonly int defaultInitialPosition = 1000;
        private readonly int defaultMovement = 4000;
        private readonly double defaultResolution = 165;

        private ICommand acceptButtonCommand;
        private ICommand cancelButtonCommand;
        private string currentResolution;
        private string desiredInitialPosition;
        private bool isAcceptButtonActive = false;
        private bool isMesuredInitialPositionHighlighted = false;
        private bool isMesuredInitialPositionTextInputActive = false;
        private bool isMesuredMovementHighlighted = false;
        private bool isMesuredMovementTextInputActive = false;
        private bool isMoveButtonActive = false;
        private bool isSetPositionButtonActive = true;
        private string mesuredInitialPosition;
        private string mesuredLenght;
        private ICommand moveButtonCommand;
        private string newResolution;
        private string noteString = Common.Resources.InstallationApp.MoveToInitialPosition;
        private string repositionLenght;
        private ICommand setPositionButtonCommand;

        #endregion Fields

        #region Constructors

        public ResolutionCalibrationVerticalAxisViewModel()
        {
            this.CurrentResolution = this.defaultResolution.ToString();
            this.DesiredInitialPosition = this.defaultInitialPosition.ToString();
            this.RepositionLenght = this.defaultMovement.ToString();
        }

        #endregion Constructors

        #region Properties

        public ICommand AcceptButtonCommand => this.acceptButtonCommand ?? (this.acceptButtonCommand = new DelegateCommand(() => this.AcceptButtonMethod()));
        public ICommand CancelButtonCommand => this.cancelButtonCommand ?? (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));
        public String CurrentResolution { get => this.currentResolution; set => this.SetProperty(ref this.currentResolution, value); }
        public String DesiredInitialPosition { get => this.desiredInitialPosition; set { this.SetProperty(ref this.desiredInitialPosition, value); } }
        public Boolean IsAcceptButtonActive { get => this.isAcceptButtonActive; set => this.SetProperty(ref this.isAcceptButtonActive, value); }
        public bool IsMesuredInitialPositionHighlighted { get => this.isMesuredInitialPositionHighlighted; set => this.SetProperty(ref this.isMesuredInitialPositionHighlighted, value); }
        public Boolean IsMesuredInitialPositionTextInputActive { get => this.isMesuredInitialPositionTextInputActive; set => this.SetProperty(ref this.isMesuredInitialPositionTextInputActive, value); }
        public Boolean IsMesuredLenghtTextInputActive { get => this.isMesuredMovementTextInputActive; set => this.SetProperty(ref this.isMesuredMovementTextInputActive, value); }
        public bool IsMesuredMovementHighlighted { get => this.isMesuredMovementHighlighted; set => this.SetProperty(ref this.isMesuredMovementHighlighted, value); }
        public Boolean IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }
        public Boolean IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }
        public String MesuredInitialPosition { get => this.mesuredInitialPosition; set { this.SetProperty(ref this.mesuredInitialPosition, value); this.CheckMesuredInitialPositionCorrectness(value); } }
        public String MesuredLenght { get => this.mesuredLenght; set { this.SetProperty(ref this.mesuredLenght, value); this.CheckMesuredRepositionLenghtCorrectness(value); } }
        public ICommand MoveButtonCommand => this.moveButtonCommand ?? (this.moveButtonCommand = new DelegateCommand(() => this.MoveButtonMethod()));
        public String NewResolution { get => this.newResolution; set => this.SetProperty(ref this.newResolution, value); }
        public String NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }
        public String RepositionLenght { get => this.repositionLenght; set { this.SetProperty(ref this.repositionLenght, value); } }
        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(() => this.SetPositionButtonMethod()));

        #endregion Properties

        #region Methods

        private void AcceptButtonMethod()
        {
            this.CurrentResolution = this.NewResolution;
            this.NoteString = Common.Resources.InstallationApp.ResolutionModified;
            var ii = DataManager.CurrentData.InstallationInfo;
            ii.Belt_Burnishing = true;
            DataManager.CurrentData.InstallationInfo = ii;
        }

        private void CalculateNewResolutionMethod()
        {
            double.TryParse(this.CurrentResolution, out var cr);
            double.TryParse(this.MesuredLenght, out var ml);
            double.TryParse(this.RepositionLenght, out var rl);
            this.NewResolution = ((cr * ml) / rl).ToString("##.##");
            this.IsAcceptButtonActive = true;
        }

        private void CancelButtonMethod()
        {
            this.DesiredInitialPosition = this.defaultInitialPosition.ToString();
            this.RepositionLenght = this.defaultMovement.ToString();
            this.MesuredLenght = "";
            this.MesuredInitialPosition = "";
            this.NewResolution = "";
            this.NoteString = Common.Resources.InstallationApp.MoveToInitialPosition;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsMesuredInitialPositionTextInputActive = false;
            this.IsMesuredLenghtTextInputActive = false;
            this.IsMoveButtonActive = false;
            this.IsSetPositionButtonActive = true;
        }

        private void CheckMesuredInitialPositionCorrectness(string input)
        {
            if (input != "")
            {
                if (Int32.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsMoveButtonActive = true;
                        this.IsMesuredInitialPositionTextInputActive = false;
                        this.IsMesuredInitialPositionHighlighted = false;
                        this.NoteString = Common.Resources.InstallationApp.MoveToPosition;
                    }
                }
            }
        }

        private void CheckMesuredRepositionLenghtCorrectness(string input)
        {
            if (input != "")
            {
                if (Int32.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsMesuredMovementHighlighted = false;
                        this.IsMesuredLenghtTextInputActive = false;
                        this.CalculateNewResolutionMethod();
                        this.NoteString = Common.Resources.InstallationApp.ConfirmResolution;
                    }
                }
            }
        }

        private async void MoveButtonMethod()
        {
            this.IsMoveButtonActive = false;
            this.NoteString = Common.Resources.InstallationApp.MovingToDesiredPosition;
            await Task.Delay(2000);
            this.IsMesuredLenghtTextInputActive = true;
            this.IsMesuredMovementHighlighted = true;
            this.NoteString = Common.Resources.InstallationApp.InsertMesuredMovement;
        }

        private async void SetPositionButtonMethod()
        {
            this.IsSetPositionButtonActive = false;
            this.NoteString = Common.Resources.InstallationApp.SettingInitialPosition;
            await Task.Delay(2000);
            this.IsMesuredInitialPositionTextInputActive = true;
            this.IsMesuredInitialPositionHighlighted = true;
            this.NoteString = Common.Resources.InstallationApp.InsertMesuredInitialPosition;
        }

        #endregion Methods
    }
}
