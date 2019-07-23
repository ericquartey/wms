using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridList : BindableBase
    {
        #region Fields

        private string description;

        private string list;

        private string machines;

        private string type;

        #endregion

        #region Properties

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public int Id { get; set; }

        public string List { get => this.list; set => this.SetProperty(ref this.list, value); }

        public string Machines { get => this.machines; set => this.SetProperty(ref this.machines, value); }

        public string Type { get => this.type; set => this.SetProperty(ref this.type, value); }

        #endregion
    }
}
