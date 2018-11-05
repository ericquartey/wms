using Prism.Mvvm;
using System;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class WeightControlViewModel : BindableBase
    {
        #region Fields

        private int acceptableWeightTolerance;
        private int feedRate;
        private int insertedWeight;
        private int mesuredWeight;
        private string noteText;
        private int testRun;

        #endregion Fields

        #region Properties

        public Int32 AcceptableWeightTolerance { get => this.acceptableWeightTolerance; set => this.SetProperty(ref this.acceptableWeightTolerance, value); }
        public Int32 FeedRate { get => this.feedRate; set => this.SetProperty(ref this.feedRate, value); }
        public Int32 InsertedWeight { get => this.insertedWeight; set => this.SetProperty(ref this.insertedWeight, value); }
        public Int32 MesuredWeight { get => this.mesuredWeight; set => this.SetProperty(ref this.mesuredWeight, value); }
        public String NoteText { get => this.noteText; set => this.SetProperty(ref this.noteText, value); }
        public Int32 TestRun { get => this.testRun; set => this.SetProperty(ref this.testRun, value); }

        #endregion Properties
    }
}
