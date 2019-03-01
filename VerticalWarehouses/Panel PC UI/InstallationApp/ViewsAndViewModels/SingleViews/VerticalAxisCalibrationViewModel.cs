using System;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Microsoft.Practices.Unity;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using System.Net;
using System.IO;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Prism.Events;
using Ferretto.VW.InstallationApp.Resources;

namespace Ferretto.VW.InstallationApp
{
    public class VerticalAxisCalibrationViewModel : BindableBase, IViewModel, IVerticalAxisCalibrationViewModel
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private InstallationHubClient installationHubClient;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private string lowerBound;

        private string noteString = Ferretto.VW.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;

        private string offset;

        private string resolution;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        private string upperBound;

        #endregion

        #region Constructors

        public VerticalAxisCalibrationViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InputsCorrectionControlEventHandler += this.CheckInputsCorrectness;
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

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public async void SubscribeMethodToEvent()
        {
            this.installationHubClient = (InstallationHubClient)this.container.Resolve<ContainerIInstallationHubClient>();
            this.installationHubClient.ReceivedMessageToAllConnectedClients += this.UpdateNoteString;
        }

        public async void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Unsubscribe((message) => { this.SubscribeMethodToEvent(); });
            this.eventAggregator.GetEvent<InstallationApp_Event>().Unsubscribe((message) => { this.UnSubscribeMethodFromEvent(); });
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
                    this.NoteString = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private void StopButtonMethod()
        {
            this.NoteString = Ferretto.VW.Resources.InstallationApp.SetOriginVerticalAxisNotCompleted;
        }

        #endregion
    }
}
