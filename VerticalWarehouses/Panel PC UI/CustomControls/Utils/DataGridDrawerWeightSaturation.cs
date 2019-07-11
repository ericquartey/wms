using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridDrawerWeightSaturation : BindableBase
    {
        #region Fields

        private string actualGrossWeight;

        private string drawer;

        private string height;

        private string maxWeight;

        private string tare;

        private string weightPercentage;

        #endregion

        #region Properties

        public string ActualGrossWeight { get => this.actualGrossWeight; set => this.SetProperty(ref this.actualGrossWeight, value); }

        public string Drawer { get => this.drawer; set => this.SetProperty(ref this.drawer, value); }

        public string Height { get => this.height; set => this.SetProperty(ref this.height, value); }

        public string MaxWeight { get => this.maxWeight; set => this.SetProperty(ref this.maxWeight, value); }

        public string Tare { get => this.tare; set => this.SetProperty(ref this.tare, value); }

        public string WeightPercentage { get => this.weightPercentage; set => this.SetProperty(ref this.weightPercentage, value); }

        #endregion
    }
}
