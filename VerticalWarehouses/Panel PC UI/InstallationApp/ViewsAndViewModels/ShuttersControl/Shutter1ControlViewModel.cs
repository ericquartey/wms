using System;
using System.Configuration;
using System.Net.Http;
using System.Windows.Input;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class Shutter1ControlViewModel : BindableBase, IShutter1ControlViewModel
    {
        #region Fields

        private string installationController = ConfigurationManager.AppSettings.Get("InstallationController");

        private string startShutter1Controller = ConfigurationManager.AppSettings.Get("InstallationStartShutter1");

        private string stopShutter1Controller = ConfigurationManager.AppSettings.Get("InstallationStopShutter1");

        private string noteString = VW.Resources.InstallationApp.Gate1Control;

        private IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive = true;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        #endregion

        #region Constructors

        public Shutter1ControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #region Properties

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(() => this.ExecuteStopButtonCommand()));

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        #endregion

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

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private async void ExecuteStartButtonCommand()
        {
            try
            {
                var client = new HttpClient();
                await client.GetStringAsync(new Uri(this.installationController + this.startShutter1Controller));
                this.IsStartButtonActive = false;
                this.IsStopButtonActive = true;
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from http get request.";
                throw;
            }
        }

        private async void ExecuteStopButtonCommand()
        {
            try
            {
                var client = new HttpClient();
                await client.GetStringAsync(new Uri(this.installationController + this.stopShutter1Controller));
                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
                this.NoteString = VW.Resources.InstallationApp.Gate1Control;
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from http get request.";
                throw;
            }
        }

        #endregion
    }
}
