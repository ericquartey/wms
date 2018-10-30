using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.Utils.Source;
using System.Windows.Media;
using System.Windows;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class ResolutionCalibrationVerticalAxisViewModel : BindableBase
    {
        #region Fields

        private readonly SolidColorBrush ferrettoGray = (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c7c4");
        private readonly SolidColorBrush ferrettoRed = (SolidColorBrush)new BrushConverter().ConvertFrom("#e2001a");

        private ICommand acceptButtonCommand;
        private ICommand cancelButtonCommand;
        private string currentResolution = "165";
        private string desiredInitialPosition;
        private SolidColorBrush input1BorderColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#e2001a");
        private SolidColorBrush input2BorderColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c7c4");
        private SolidColorBrush input3BorderColor = (SolidColorBrush)new BrushConverter().ConvertFrom("#c5c7c4");
        private bool isAcceptButtonActive;
        private bool isMesuredLenghtTextInputActive;
        private bool isMoveButtonActive;
        private bool isRepositionTextInputActive;
        private bool isSetPositionButtonActive;
        private string mesuredLenght;
        private ICommand moveButtonCommand;
        private string newResolution;
        private string noteString = Common.Resources.InstallationApp.InsertiDesiredInitialPosition;
        private string repositionLenght;
        private ICommand setPositionButtonCommand;

        #endregion Fields

        #region Properties

        public ICommand AcceptButtonCommand => this.acceptButtonCommand ?? (this.acceptButtonCommand = new DelegateCommand(() => this.AcceptButtonMethod()));
        public ICommand CancelButtonCommand => this.cancelButtonCommand ?? (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));
        public String CurrentResolution { get => this.currentResolution; set => this.SetProperty(ref this.currentResolution, value); }
        public String DesiredInitialPosition { get => this.desiredInitialPosition; set { this.SetProperty(ref this.desiredInitialPosition, value); this.CheckDesiredPositionCorrectness(this.desiredInitialPosition); } }
        public SolidColorBrush Input1BorderColor { get => this.input1BorderColor; set => this.SetProperty(ref this.input1BorderColor, value); }
        public SolidColorBrush Input2BorderColor { get => this.input2BorderColor; set => this.SetProperty(ref this.input2BorderColor, value); }
        public SolidColorBrush Input3BorderColor { get => this.input3BorderColor; set => this.SetProperty(ref this.input3BorderColor, value); }
        public Boolean IsAcceptButtonActive { get => this.isAcceptButtonActive; set => this.SetProperty(ref this.isAcceptButtonActive, value); }
        public Boolean IsMesuredLenghtTextInputActive { get => this.isMesuredLenghtTextInputActive; set => this.SetProperty(ref this.isMesuredLenghtTextInputActive, value); }
        public Boolean IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }
        public Boolean IsRepositionTextInputActive { get => this.isRepositionTextInputActive; set => this.SetProperty(ref this.isRepositionTextInputActive, value); }
        public Boolean IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }
        public String MesuredLenght { get => this.mesuredLenght; set { this.SetProperty(ref this.mesuredLenght, value); this.CalculateNewResolutionMethod(); } }
        public ICommand MoveButtonCommand => this.moveButtonCommand ?? (this.moveButtonCommand = new DelegateCommand(() => this.MoveButtonMethod()));
        public String NewResolution { get => this.newResolution; set => this.SetProperty(ref this.newResolution, value); }
        public String NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }
        public String RepositionLenght { get => this.repositionLenght; set { this.SetProperty(ref this.repositionLenght, value); this.CheckRepositionLenghtCorrectness(this.repositionLenght); } }
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
            double.TryParse(this.RepositionLenght, out var tl);
            this.NewResolution = ((cr * ml) / tl).ToString("##.##");
            this.Input3BorderColor = this.ferrettoGray;
            this.IsAcceptButtonActive = true;
        }

        private void CancelButtonMethod()
        {
            this.DesiredInitialPosition = "";
            this.RepositionLenght = "";
            this.MesuredLenght = "";
            this.NewResolution = "";
            this.NoteString = Common.Resources.InstallationApp.InsertiDesiredInitialPosition;
            this.IsAcceptButtonActive = false;
            this.Input1BorderColor = this.ferrettoRed;
            this.IsMesuredLenghtTextInputActive = false;
            this.IsMoveButtonActive = false;
            this.IsRepositionTextInputActive = false;
            this.IsSetPositionButtonActive = false;
        }

        private void CheckDesiredPositionCorrectness(string input)
        {
            if (input != "")
            {
                if (Int32.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsSetPositionButtonActive = true;
                        this.Input1BorderColor = this.ferrettoGray;
                    }
                    else
                    {
                        this.IsSetPositionButtonActive = false;
                    }
                }
                else
                {
                    this.IsSetPositionButtonActive = false;
                }
            }
            else
            {
                this.IsSetPositionButtonActive = false;
            }
        }

        private void CheckRepositionLenghtCorrectness(string input)
        {
            if (input != "")
            {
                if (Int32.TryParse(input, out var i))
                {
                    if (i > 0)
                    {
                        this.IsMoveButtonActive = true;
                        this.Input2BorderColor = this.ferrettoGray;
                    }
                    else
                    {
                        this.IsMoveButtonActive = false;
                    }
                }
                else
                {
                    this.IsMoveButtonActive = false;
                }
            }
            else
            {
                this.IsMoveButtonActive = false;
            }
        }

        private async void MoveButtonMethod()
        {
            this.IsMoveButtonActive = false;
            this.NoteString = "Moving to desired position...";
            await Task.Delay(2000);
            this.IsMesuredLenghtTextInputActive = true;
            this.Input3BorderColor = this.ferrettoRed;
            this.NoteString = Common.Resources.InstallationApp.InsertMesuredMovement;
        }

        private async void SetPositionButtonMethod()
        {
            this.IsSetPositionButtonActive = false;
            this.NoteString = "Setting initial position...";
            await Task.Delay(2000);
            this.IsRepositionTextInputActive = true;
            this.Input2BorderColor = this.ferrettoRed;
            this.NoteString = Common.Resources.InstallationApp.InsertDesiredMovement;
        }

        #endregion Methods
    }
}
