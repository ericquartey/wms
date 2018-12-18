using System;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class Gate1HeightControlViewModel : BindableBase
    {
        #region Fields

        private int activeRaysQuantity;
        private double currentHeight;
        private double gateCorrection;
        private string noteText;
        private int speed;
        private double systemError;
        private double tolerance;

        #endregion Fields

        #region Properties

        public Int32 ActiveRaysQuantity { get => this.activeRaysQuantity; set => this.SetProperty(ref this.activeRaysQuantity, value); }

        public Double CurrentHeight { get => this.currentHeight; set => this.SetProperty(ref this.currentHeight, value); }

        public Double GateCorrection { get => this.gateCorrection; set => this.SetProperty(ref this.gateCorrection, value); }

        public String NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }

        public Int32 Speed { get => this.speed; set => this.SetProperty(ref this.speed, value); }

        public Double SystemError { get => this.systemError; set => this.SetProperty(ref this.systemError, value); }

        public Double Tolerance { get => this.tolerance; set => this.SetProperty(ref this.tolerance, value); }

        #endregion Properties
    }
}
