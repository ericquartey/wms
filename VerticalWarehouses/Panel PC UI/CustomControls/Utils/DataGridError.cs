using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class ErrorStatistics : BindableBase
    {
        #region Fields

        private string error;

        private string total;

        private string totalPercentage;

        #endregion

        #region Properties

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public string Total { get => this.total; set => this.SetProperty(ref this.total, value); }

        public string TotalPercentage { get => this.totalPercentage; set => this.SetProperty(ref this.totalPercentage, value); }

        #endregion
    }
}
