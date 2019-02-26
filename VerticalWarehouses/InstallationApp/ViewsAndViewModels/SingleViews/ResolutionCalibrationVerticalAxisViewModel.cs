using System;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Ferretto.VW.MathLib;
using Prism.Mvvm;
using Prism.Events;

namespace Ferretto.VW.InstallationApp
{
    public class ResolutionCalibrationVerticalAxisViewModel : BindableBase, IViewModel, IResolutionCalibrationVerticalAxisViewModel, IViewModelRequiresContainer
    {
        #region Fields

        public IUnityContainer Container;

        private readonly int defaultInitialPosition = 1000;

        private readonly int defaultMovement = 4000;

        private ICommand acceptButtonCommand;

        private ICommand cancelButtonCommand;

        private string currentResolution;

        private string desiredInitialPosition;

        private decimal desiredInitialPositionDec;

        private IEventAggregator eventAggregator;

        private bool isAcceptButtonActive;

        private bool isMesuredInitialPositionHighlighted;

        private bool isMesuredInitialPositionTextInputActive;

        private bool isMesuredMovementHighlighted;

        private bool isMesuredMovementTextInputActive;

        private bool isMoveButtonActive;

        private bool isSetPositionButtonActive;

        private string mesuredInitialPosition;

        private string mesuredLenght;

        private ICommand moveButtonCommand;

        private string newResolution;

        private string noteString = Resources.InstallationApp.MoveToInitialPosition;

        private string repositionLenght;

        private ICommand setPositionButtonCommand;

        #endregion

        #region Constructors

        public ResolutionCalibrationVerticalAxisViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public ICommand AcceptButtonCommand => this.acceptButtonCommand ?? (this.acceptButtonCommand = new DelegateCommand(() => this.AcceptButtonMethod()));

        public ICommand CancelButtonCommand => this.cancelButtonCommand ?? (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));

        public string CurrentResolution { get => this.currentResolution; set => this.SetProperty(ref this.currentResolution, value); }

        public string DesiredInitialPosition { get => this.desiredInitialPosition; set => this.SetProperty(ref this.desiredInitialPosition, value); }

        public bool IsAcceptButtonActive { get => this.isAcceptButtonActive; set => this.SetProperty(ref this.isAcceptButtonActive, value); }

        public bool IsMesuredInitialPositionHighlighted { get => this.isMesuredInitialPositionHighlighted; set => this.SetProperty(ref this.isMesuredInitialPositionHighlighted, value); }

        public bool IsMesuredInitialPositionTextInputActive { get => this.isMesuredInitialPositionTextInputActive; set => this.SetProperty(ref this.isMesuredInitialPositionTextInputActive, value); }

        public bool IsMesuredLenghtTextInputActive { get => this.isMesuredMovementTextInputActive; set => this.SetProperty(ref this.isMesuredMovementTextInputActive, value); }

        public bool IsMesuredMovementHighlighted { get => this.isMesuredMovementHighlighted; set => this.SetProperty(ref this.isMesuredMovementHighlighted, value); }

        public bool IsMoveButtonActive { get => this.isMoveButtonActive; set => this.SetProperty(ref this.isMoveButtonActive, value); }

        public bool IsSetPositionButtonActive { get => this.isSetPositionButtonActive; set => this.SetProperty(ref this.isSetPositionButtonActive, value); }

        public string MesuredInitialPosition { get => this.mesuredInitialPosition; set { this.SetProperty(ref this.mesuredInitialPosition, value); this.CheckMesuredInitialPositionCorrectness(value); } }

        public string MesuredLenght { get => this.mesuredLenght; set { this.SetProperty(ref this.mesuredLenght, value); this.CheckMesuredRepositionLenghtCorrectness(value); } }

        public ICommand MoveButtonCommand => this.moveButtonCommand ?? (this.moveButtonCommand = new DelegateCommand(() => this.MoveButtonMethod()));

        public string NewResolution { get => this.newResolution; set => this.SetProperty(ref this.newResolution, value); }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public string RepositionLenght { get => this.repositionLenght; set => this.SetProperty(ref this.repositionLenght, value); }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(() => this.SetPositionButtonMethod()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO implement feature
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
        }

        public void PositioningDone(bool result)
        {
            // TODO implement feature
        }

        public void SubscribeMethodToEvent()
        {
            // TODO implement feature
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO implement feature
        }

        private void AcceptButtonMethod()
        {
            // TODO implement feature
        }

        private void CalculateNewResolutionMethod()
        {
            decimal.TryParse(this.CurrentResolution, out var cr);
            decimal.TryParse(this.MesuredLenght, out var ml);
            decimal.TryParse(this.RepositionLenght, out var rl);
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
            this.NoteString = Resources.InstallationApp.MoveToInitialPosition;
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
                        this.NoteString = Resources.InstallationApp.MoveToPosition;
                        this.IsMesuredInitialPositionTextInputActive = true;
                        this.IsMesuredInitialPositionHighlighted = true;
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
                        this.NoteString = Resources.InstallationApp.ConfirmResolution;
                    }
                }
            }
        }

        private void MoveButtonMethod()
        {
            // TODO implement feature
        }

        private void SetPositionButtonMethod()
        {
            // TODO implement feature
        }

        #endregion
    }
}
