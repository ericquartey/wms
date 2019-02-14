using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Microsoft.Practices.Unity;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using System.Net;
using System.IO;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IViewModel, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        public IUnityContainer Container;

        private InstallationHubClient installationClient;

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
            this.installationClient = (InstallationHubClient)this.Container.Resolve<IInstallationHubClient>();
        }

        public void SubscribeMethodToEvent()
        {
            try
            {
                this.installationClient.InitializeInstallationHubClient("https://localhost://5001", "/automation-endpoint");
                this.installationClient.ConnectAsync();

                this.installationClient.ReceivedMessageToAllConnectedClients += this.UpdateNoteString;
                this.NoteString = "Connected to Installation Hub";
            }
            catch (ArgumentNullException nullException)
            {
                if (this.installationClient == null)
                {
                    this.installationClient = (InstallationHubClient)this.Container.Resolve<IInstallationHubClient>();
                    this.installationClient.ConnectAsync();
                    this.installationClient.ReceivedMessageToAllConnectedClients += this.UpdateNoteString;
                }
                else
                {
                    throw nullException;
                }
            }
            catch (Exception exception)
            {
                this.NoteString = "Did not connect to Installation Hub";
                throw exception;
            }
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.installationClient.DisconnectAsync();
        }

        public void UpdateNoteString(object sender, string message)
        {
            this.NoteString = message;
        }

        private void CheckInputsCorrectness()
        {
            if (int.TryParse(this.LowerBound, out var _lowerBound) &&
                int.TryParse(this.Offset, out var _offset) &&
                int.TryParse(this.Resolution, out var _resolution) &&
                int.TryParse(this.UpperBound, out var _upperBound))
            { // TODO: DEFINE AND INSERT VALIDATION LOGIC IN HERE. THESE PROPOSITIONS ARE TEMPORARY
                this.IsStartButtonActive = (_lowerBound > 0 && _lowerBound < _upperBound && _upperBound > 0 && _resolution > 0 && _offset > 0) ? true : false;
            }
            else
            {
                this.IsStartButtonActive = false;
            }
        }

        private void ExecuteStartButtonCommand()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://localhost:5001/api/Test/HomingTest");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    reader.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
            }
        }

        private void StopButtonMethod()
        {
            this.NoteString = Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
        }

        #endregion
    }
}
