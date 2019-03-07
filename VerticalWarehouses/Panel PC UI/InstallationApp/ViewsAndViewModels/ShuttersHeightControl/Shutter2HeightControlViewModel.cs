using System;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class Shutter2HeightControlViewModel : BindableBase, IShutter2HeightControlViewModel
    {
        #region Fields

        private int activeRaysQuantity;

        private decimal currentHeight;

        private IEventAggregator eventAggregator;

        private decimal gateCorrection;

        private string noteText;

        private int speed;

        private decimal systemError;

        private decimal tolerance;

        #endregion

        #region Constructors

        public Shutter2HeightControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public int ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public decimal CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public decimal GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public string NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public int Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

        public decimal SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public decimal Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
