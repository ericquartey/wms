using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridDrawer : BindableBase
    {
        #region Fields

        private string cell;

        private string drawer;

        private string height;

        private string side;

        private string state;

        private string weight;

        #endregion

        #region Properties

        public string Cell { get => this.cell; set => this.SetProperty(ref this.cell, value); }

        public string Drawer { get => this.drawer; set => this.SetProperty(ref this.drawer, value); }

        public string Height { get => this.height; set => this.SetProperty(ref this.height, value); }

        public string Side { get => this.side; set => this.SetProperty(ref this.side, value); }

        public string State { get => this.state; set => this.SetProperty(ref this.state, value); }

        public string Weight { get => this.weight; set => this.SetProperty(ref this.weight, value); }

        #endregion
    }
}
