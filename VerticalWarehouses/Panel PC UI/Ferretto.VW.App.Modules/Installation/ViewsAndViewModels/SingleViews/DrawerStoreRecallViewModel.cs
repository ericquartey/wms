using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class DrawerStoreRecallViewModel : BindableBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineLoadingUnitsService loadingUnitsService;

        private bool isStartRecallButtonActive = true;

        private bool isStartStoreButtonActive = true;

        private bool isStopButtonActive;

        private string noteString = "No operation";

        private ICommand startRecallButtonCommand;

        private ICommand startStoreButtonCommand;

        private ICommand stopButtonCommand;

        #endregion

        #region Constructors

        public DrawerStoreRecallViewModel(
            IEventAggregator eventAggregator,
            IMachineLoadingUnitsService loadingUnitsService)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (loadingUnitsService is null)
            {
                throw new ArgumentNullException(nameof(loadingUnitsService));
            }

            this.eventAggregator = eventAggregator;
            this.loadingUnitsService = loadingUnitsService;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public bool IsStartRecallButtonActive { get => this.isStartRecallButtonActive; set => this.SetProperty(ref this.isStartRecallButtonActive, value); }

        public bool IsStartStoreButtonActive { get => this.isStartStoreButtonActive; set => this.SetProperty(ref this.isStartStoreButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteString { get => this.noteString; set => this.SetProperty(ref this.noteString, value); }

        public ICommand StartRecallButtonCommand => this.startRecallButtonCommand ?? (this.startRecallButtonCommand = new DelegateCommand(async () => await this.StartRecallButtonAsync()));

        public ICommand StartStoreButtonCommand => this.startStoreButtonCommand ?? (this.startStoreButtonCommand = new DelegateCommand(async () => await this.StartStoreButtonAsync()));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(async () => await this.StopButtonMethodAsync()));

        #endregion

        #region Methods

        private async Task StartRecallButtonAsync()
        {
            try
            {
                this.IsStartRecallButtonActive = false;
                this.IsStopButtonActive = true;

                await this.loadingUnitsService.StartMovingAsync(DrawerOperation.ManualRecall);

                this.NoteString = "Start drawer recall...";
            }
            catch (Exception)
            {
                this.NoteString = "Couldn't get response from this http get request.";
                throw;
            }
        }

        private async Task StartStoreButtonAsync()
        {
            try
            {
                this.IsStartStoreButtonActive = false;
                this.IsStopButtonActive = true;

                await this.loadingUnitsService.StartMovingAsync(DrawerOperation.ManualStore);

                this.NoteString = "Start drawer storing...";
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
                await this.loadingUnitsService.StopAsync();

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
