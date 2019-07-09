using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.App.Controls.Utils
{
    public class DataGridKit : BindableBase
    {
        #region Fields

        private string description;

        private string kit;

        private string request;

        private string state;

        #endregion

        #region Properties

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public string Kit { get => this.kit; set => this.SetProperty(ref this.kit, value); }

        public string Request { get => this.request; set => this.SetProperty(ref this.request, value); }

        public string State { get => this.state; set => this.SetProperty(ref this.state, value); }

        #endregion
    }
}
