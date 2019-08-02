using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public class IOsModel : BindableBase
    {
        #region Fields

        private string description;

        private bool value;

        #endregion

        #region Constructors

        public IOsModel()
        {
        }

        public IOsModel(string description, bool value)
        {
            this.Description = description;
            this.Value = value;
        }

        #endregion

        #region Properties

        public string Description { get => this.description; set => this.SetProperty(ref this.description, value); }

        public bool Value { get => this.value; set => this.SetProperty(ref this.value, value); }

        #endregion
    }
}
