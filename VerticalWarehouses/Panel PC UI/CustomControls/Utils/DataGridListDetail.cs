using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.CustomControls.Utils
{
    public class DataGridListDetail : BindableBase
    {
        #region Fields

        private string description;

        private string item;

        private string machine;

        private string quantity;

        private string row;

        #endregion

        #region Properties

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public string Item { get => this.item; set => this.SetProperty(ref this.item, value); }

        public string Machine { get => this.machine; set => this.SetProperty(ref this.machine, value); }

        public string Quantity { get => this.quantity; set => this.SetProperty(ref this.quantity, value); }

        public string Row { get => this.row; set => this.SetProperty(ref this.row, value); }

        #endregion
    }
}
