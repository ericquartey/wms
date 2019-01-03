using System;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Ferretto.VW.Utils.Source;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Ferretto.VW.MathLib;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class ResolutionCalibrationVerticalAxisViewModel : BindableBase, IViewModel, IResolutionCalibrationVerticalAxisViewModel, IViewModelRequiresContainer
    {
        #region Fields

        private readonly int defaultInitialPosition = 1000;
        private readonly int defaultMovement = 4000;

        private float acc = 1;
        private ICommand acceptButtonCommand;
        private ICommand cancelButtonCommand;
        private IUnityContainer container;
        private Converter converter;
        private string currentResolution;

        private DataManager data;

        // Temporary assigned to constant value, they will become variable with new funcionalities
        private float dec = 1;

        private string desiredInitialPosition;
        private decimal desiredInitialPositionDec;
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
        private short offset = 1;

        // Temporary assigned to constant value, they will become variable with new funcionalities
        private bool operation;

        private PositioningDrawer positioningDrawer;
        private string repositionLenght;
        private decimal resolution;
        private ICommand setPositionButtonCommand;
        private float vMax = 1;

        // Temporary assigned to constant value, they will become variable with new funcionalities
        private float w = 1;

        private decimal x;

        #endregion Fields

        #region Constructors

        // Temporary assigned to constant value, they will become variable with new funcionalities
        public ResolutionCalibrationVerticalAxisViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public ICommand AcceptButtonCommand => this.acceptButtonCommand ?? (this.acceptButtonCommand = new DelegateCommand(() => this.AcceptButtonMethod()));

        public ICommand CancelButtonCommand => this.cancelButtonCommand ?? (this.cancelButtonCommand = new DelegateCommand(() => this.CancelButtonMethod()));

        public String CurrentResolution { get => this.currentResolution; set => this.SetProperty(ref this.currentResolution, value); }

        public String DesiredInitialPosition { get => this.desiredInitialPosition; set => this.SetProperty(ref this.desiredInitialPosition, value); }

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

        public String RepositionLenght { get => this.repositionLenght; set => this.SetProperty(ref this.repositionLenght, value); }

        public ICommand SetPositionButtonCommand => this.setPositionButtonCommand ?? (this.setPositionButtonCommand = new DelegateCommand(() => this.SetPositionButtonMethod()));

        #endregion Properties

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
            if (this.positioningDrawer != null)
            {
                this.positioningDrawer.Stop();
            }
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.container = _container;
            bool conversionResolution;

            this.positioningDrawer = (PositioningDrawer)this.container.Resolve<IPositioningDrawer>();
            this.converter = (Converter)this.container.Resolve<IConverter>();
            this.data = (DataManager)this.container.Resolve<IDataManager>();

            this.CurrentResolution = this.converter.ManageResolution.ToString("##.##");
            this.DesiredInitialPosition = this.defaultInitialPosition.ToString();
            this.RepositionLenght = this.defaultMovement.ToString();

            conversionResolution = decimal.TryParse(this.CurrentResolution, out this.resolution);

            if (!conversionResolution)
            {
                this.NoteString = "Wrong resolution";
            }
        }

        public void PositioningDone(bool result)
        {
            var message = "";

            if (result)
            {
                // If operation = true -> SetPosition
                if (this.operation)
                {
                    this.IsMesuredInitialPositionTextInputActive = true;
                    this.IsMesuredInitialPositionHighlighted = true;
                    message = Common.Resources.InstallationApp.InsertMesuredInitialPosition;
                }
                else // false -> Move
                {
                    this.IsMesuredLenghtTextInputActive = true;
                    this.IsMesuredMovementHighlighted = true;
                    message = Common.Resources.InstallationApp.InsertMesuredMovement;
                }
            }
            else
            {
                if (this.operation)
                {
                    message = "Initial position not done";
                }
                else
                {
                    message = "Positioning not done";
                }
            }

            this.NoteString = message;
        }

        public void SubscribeMethodToEvent()
        {
            if (this.positioningDrawer != null)
            {
                this.positioningDrawer.ThrowEndEvent += this.PositioningDone;
            }
        }

        public void UnSubscribeMethodFromEvent()
        {
            if (this.positioningDrawer != null)
            {
                this.positioningDrawer.ThrowEndEvent -= this.PositioningDone;
            }
        }

        private void AcceptButtonMethod()
        {
            this.CurrentResolution = this.NewResolution;
            this.NoteString = Common.Resources.InstallationApp.ResolutionModified;
            var ii = this.data.InstallationInfo;
            ii.Belt_Burnishing = true;
            this.data.InstallationInfo = ii;

            decimal.TryParse(this.NewResolution, out var resolutionDec);
            this.converter.ManageResolution = resolutionDec;
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
            this.NoteString = Common.Resources.InstallationApp.MoveToInitialPosition;
            this.IsAcceptButtonActive = false;
            this.IsMesuredInitialPositionHighlighted = false;
            this.IsMesuredInitialPositionTextInputActive = false;
            this.IsMesuredLenghtTextInputActive = false;
            this.IsMoveButtonActive = false;
            this.IsSetPositionButtonActive = true;

            if (this.positioningDrawer != null)
            {
                this.positioningDrawer.Stop();
            }
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

                        // Inizio modifica
                        this.IsMesuredInitialPositionTextInputActive = true;
                        this.IsMesuredInitialPositionHighlighted = true;
                        // Fine modifica
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

        private void MoveButtonMethod()
        {
            // Begin changes for the PositioningDrawer
            bool conversionRepositionLenght;

            conversionRepositionLenght = decimal.TryParse(this.RepositionLenght, out var repositionLenghtDec);

            if (conversionRepositionLenght)
            {
                this.operation = false;
                this.x = repositionLenghtDec + this.desiredInitialPositionDec;
                this.IsMoveButtonActive = false;
                this.NoteString = Common.Resources.InstallationApp.MovingToDesiredPosition;
                this.positioningDrawer.ThrowEndEvent += this.PositioningDone;
                this.positioningDrawer.AbsoluteMovement = true;
                this.positioningDrawer.MoveAlongVerticalAxisToPoint(this.x, this.vMax, this.acc, this.dec, this.w, this.offset);
            }
            else
            {
                this.NoteString = "Positioning not possible";
            }
            // End changes for the PositioningDrawer
        }

        private void SetPositionButtonMethod()
        {
            if (decimal.TryParse(this.desiredInitialPosition, out this.desiredInitialPositionDec))
            {
                this.operation = true;
                this.x = this.desiredInitialPositionDec;
                this.IsSetPositionButtonActive = false;
                this.NoteString = Common.Resources.InstallationApp.SettingInitialPosition;
                if (this.positioningDrawer != null)
                {
                    this.positioningDrawer.AbsoluteMovement = true;
                    this.positioningDrawer.MoveAlongVerticalAxisToPoint(this.x, this.vMax, this.acc, this.dec, this.w, this.offset);
                }
            }
            // End changes for the initial positioning
        }

        #endregion Methods
    }
}
