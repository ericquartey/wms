using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridDrawerSaturation : BindableBase
    {
        #region Fields

        private string compartments;

        private string drawerId;

        private string filling;

        private string fillingPercentage;

        private string missions;

        #endregion

        #region Properties

        public string Compartments { get => this.compartments; set => this.SetProperty(ref this.compartments, value); }

        public string DrawerId { get => this.drawerId; set => this.SetProperty(ref this.drawerId, value); }

        public string Filling { get => this.filling; set => this.SetProperty(ref this.filling, value); }

        public string FillingPercentage { get => this.fillingPercentage; set => this.SetProperty(ref this.fillingPercentage, value); }

        public string Missions { get => this.missions; set => this.SetProperty(ref this.missions, value); }

        #endregion
    }
}
