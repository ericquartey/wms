using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    public class DrawerStoreRecallViewModel : BindableBase, IDrawerStoreRecallViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private bool isStartRecallButtonActive = true;

        private bool isStartStoreButtonActive = true;

        private bool isStopButtonActive;

        private string noteString = "No operation";

        private ICommand startRecallButtonCommand;

        private ICommand startStoreButtonCommand;

        private ICommand stopButtonCommand;

        #endregion

        #region Constructors

        public DrawerStoreRecallViewModel(IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public bool IsStartRecallButtonActive { get => this.isStartRecallButtonActive; set => this.SetProperty(ref this.isStartRecallButtonActive, value); }

        public bool IsStartStoreButtonActive { get => this.isStartStoreButtonActive; set => this.SetProperty(ref this.isStartStoreButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public ICommand StartRecallButtonCommand => this.startRecallButtonCommand ?? (this.startRecallButtonCommand = new DelegateCommand(async () => await this.ExecuteStartRecallButtonCommandAsync()));

        public ICommand StartStoreButtonCommand => this.startStoreButtonCommand ?? (this.startStoreButtonCommand = new DelegateCommand(async () => await this.ExecuteStartStoreButtonCommandAsync()));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopButtonMethodAsync()));

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            //TODO Add your implementation code here
        }

        public Task OnEnterViewAsync()
        {
            //TODO Add your implementation code here
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            //TODO Add your implementation code here
        }

        private async Task ExecuteStartRecallButtonCommandAsync()
        {
            try
            {
                this.IsStartRecallButtonActive = false;
                this.IsStopButtonActive = true;

                // TODO Call the method from service
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private async Task ExecuteStartStoreButtonCommandAsync()
        {
            try
            {
                this.IsStartStoreButtonActive = false;
                this.IsStopButtonActive = true;

                // TODO Call the method from service
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private async Task StopButtonMethodAsync()
        {
            try
            {
                // TODO Call the method from service

                this.IsStartStoreButtonActive = true;
                this.IsStartRecallButtonActive = true;
                this.IsStopButtonActive = false;

                this.NoteString = "No operation executed.";
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        #endregion
    }
}
