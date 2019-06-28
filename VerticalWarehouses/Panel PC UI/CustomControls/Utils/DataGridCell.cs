using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Utils
{
    public class DataGridCell : BindableBase
    {
        #region Fields

        private string state;

        private string totalBack;

        private string totalBackPercentage;

        private string totalFront;

        private string totalFrontPercentage;

        #endregion

        #region Constructors

        public DataGridCell()
        {
        }

        public DataGridCell(string state, string totalFront, string totalFrontPercentage, string totalBack, string totalBackPercentage)
        {
            this.State = state;
            this.TotalFront = totalFront;
            this.TotalFrontPercentage = totalFrontPercentage;
            this.TotalBack = totalBack;
            this.TotalBackPercentage = totalBackPercentage;
        }

        #endregion

        #region Properties

        public string State { get => this.state; set => this.SetProperty(ref this.state, value); }

        public string TotalBack { get => this.totalBack; set => this.SetProperty(ref this.totalBack, value); }

        public string TotalBackPercentage { get => this.totalBackPercentage; set => this.SetProperty(ref this.totalBackPercentage, value); }

        public string TotalFront { get => this.totalFront; set => this.SetProperty(ref this.totalFront, value); }

        public string TotalFrontPercentage { get => this.totalFrontPercentage; set => this.SetProperty(ref this.totalFrontPercentage, value); }

        #endregion
    }
}
