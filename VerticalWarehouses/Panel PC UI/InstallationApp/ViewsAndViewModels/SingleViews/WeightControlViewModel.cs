﻿namespace Ferretto.VW.InstallationApp
{
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Prism.Commands;
    using Prism.Events;
    using Prism.Mvvm;
    using Unity;

    public class WeightControlViewModel : BindableBase, IWeightControlViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private int acceptableWeightTolerance;

        private ICommand beginButtonCommand;

        private IUnityContainer container;

        private ICommand exitFromViewCommand;

        private int feedRate;

        private int insertedWeight;

        private bool isSetBeginButtonActive = true;

        private bool isSetStopButtonActive = true;

        private int mesuredWeight;

        private string noteText = App.Resources.InstallationApp.WeightControl;

        private ICommand stopButtonCommand;

        private int testRun;

        #endregion

        #region Constructors

        public WeightControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public int AcceptableWeightTolerance { get => this.acceptableWeightTolerance; set => this.SetProperty(ref this.acceptableWeightTolerance, value); }

        public ICommand BeginButtonCommand => this.beginButtonCommand ?? (this.beginButtonCommand = new DelegateCommand(this.BeginButtonCommandMethod));

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public int FeedRate { get => this.feedRate; set => this.SetProperty(ref this.feedRate, value); }

        public int InsertedWeight { get => this.insertedWeight; set => this.SetProperty(ref this.insertedWeight, value); }

        public bool IsSetBeginButtonActive { get => this.isSetBeginButtonActive; set => this.SetProperty(ref this.isSetBeginButtonActive, value); }

        public bool IsSetStopButtonActive { get => this.isSetStopButtonActive; set => this.SetProperty(ref this.isSetStopButtonActive, value); }

        public int MesuredWeight { get => this.mesuredWeight; set => this.SetProperty(ref this.mesuredWeight, value); }

        public BindableBase NavigationViewModel { get; set; }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(this.StopButtonCommandMethod));

        public int TestRun { get => this.testRun; set => this.SetProperty(ref this.testRun, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private void BeginButtonCommandMethod()
        {
            this.IsSetStopButtonActive = true;
            this.IsSetBeginButtonActive = false;
        }

        private void StopButtonCommandMethod()
        {
            // TODO
        }

        #endregion
    }
}
