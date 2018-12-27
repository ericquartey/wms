using System;
using System.Threading;
using System.Windows.Input;
using Ferretto.VW.ActionBlocks;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class WeightControlViewModel : BindableBase, IViewModel
    {
        #region Fields

        private int acceptableWeightTolerance;
        private bool executeWeightRun;
        private ICommand exitFromViewCommand;
        private int feedRate;
        private int insertedWeight;
        private bool isSetBeginButtonActive = true;
        private bool isSetStopButtonActive = false;
        private int mesuredWeight;
        private string noteText = Common.Resources.InstallationApp.WeightControl;
        private AutoResetEvent raiseRestorePositionEvent;
        private RegisteredWaitHandle regWaitForRestorePositionThread;
        private ICommand setBeginButtonCommand;
        private ICommand setStopButtonCommand;
        private int testRun;

        #endregion Fields

        #region Constructors

        public WeightControlViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public Int32 AcceptableWeightTolerance { get => this.acceptableWeightTolerance; set => this.SetProperty(ref this.acceptableWeightTolerance, value); }

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public Int32 FeedRate { get => this.feedRate; set => this.SetProperty(ref this.feedRate, value); }

        public Int32 InsertedWeight { get => this.insertedWeight; set => this.SetProperty(ref this.insertedWeight, value); }

        public Boolean IsSetBeginButtonActive { get => this.isSetBeginButtonActive; set => this.SetProperty(ref this.isSetBeginButtonActive, value); }

        public Boolean IsSetStopButtonActive { get => this.isSetStopButtonActive; set => this.SetProperty(ref this.isSetStopButtonActive, value); }

        public Int32 MesuredWeight { get => this.mesuredWeight; set => this.SetProperty(ref this.mesuredWeight, value); }

        public String NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public ICommand SetBeginButtonCommand => this.setBeginButtonCommand ?? (this.setBeginButtonCommand = new DelegateCommand(this.SetBeginButtonCommandMethod));

        public ICommand SetStopButtonCommand => this.setStopButtonCommand ?? (this.setStopButtonCommand = new DelegateCommand(this.SetStopButtonCommandMethod));

        public Int32 TestRun { get => this.testRun; set => this.SetProperty(ref this.testRun, value); }

        #endregion Properties

        #region Methods

        public void ExitFromViewMethod()
        {
            // Ensure to stop the movement
            ActionManager.DrawerWeightDetectionInstance.Stop();
            // Unsubscribe methods
            this.UnSubscribeMethodFromEvent();
        }

        public void SubscribeMethodToEvent()
        {
            ActionManager.DrawerWeightDetectionInstance.EndEvent += this.WeightDetectionDone;
            ActionManager.DrawerWeightDetectionInstance.ErrorEvent += this.WeightDetectionError;
        }

        public void UnSubscribeMethodFromEvent()
        {
            ActionManager.DrawerWeightDetectionInstance.ErrorEvent -= this.WeightDetectionError;
            ActionManager.DrawerWeightDetectionInstance.EndEvent -= this.WeightDetectionDone;
        }

        private void createWaitThreadForRestorePosition()
        {
            this.raiseRestorePositionEvent = new AutoResetEvent(false);
            this.regWaitForRestorePositionThread = ThreadPool.RegisterWaitForSingleObject(this.raiseRestorePositionEvent, this.onManageRestorePosition, null, -1, false);
        }

        private void destroyWaitThreadForRestorePosition()
        {
            this.regWaitForRestorePositionThread?.Unregister(this.raiseRestorePositionEvent);
        }

        private void onManageRestorePosition(object data, bool bTimeOut)
        {
            if (bTimeOut)
            {
            }
            else
            {
                ActionManager.DrawerWeightDetectionInstance.RestorePosition();
            }
        }

        private void SetBeginButtonCommandMethod()
        {
            this.executeWeightRun = false;

            this.createWaitThreadForRestorePosition();

            // Perform the routine to weight
            ActionManager.DrawerWeightDetectionInstance.Run(this.TestRun, this.FeedRate, 1.0f, 1.0f);

            this.IsSetStopButtonActive = true;
            this.IsSetBeginButtonActive = false;
        }

        private void SetStopButtonCommandMethod()
        {
            this.destroyWaitThreadForRestorePosition();

            // Stop to movement
            ActionManager.DrawerWeightDetectionInstance.Stop();
        }

        private void WeightDetectionDone(Boolean result)
        {
            if (!this.executeWeightRun)
            {
                // display the weight
                this.MesuredWeight = (int)ActionManager.DrawerWeightDetectionInstance.Weight;

                // check tolerance
                if (this.InsertedWeight != 0)
                {
                    float tolerance = this.MesuredWeight / this.InsertedWeight;
                    if ((Math.Abs(tolerance) / 100.0f) < this.AcceptableWeightTolerance)
                    {
                        // Do somethig! Display a message?!
                    }
                }

                this.executeWeightRun = true;
                // Raise event to restore position
                this.raiseRestorePositionEvent?.Set();
            }
            else
            {
                this.destroyWaitThreadForRestorePosition();

                // restore interface
                this.IsSetBeginButtonActive = true;
                this.IsSetStopButtonActive = false;
            }
        }

        private void WeightDetectionError(String error_Message)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}
