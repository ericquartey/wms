using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public class BitModel : BindableBase
    {
        #region Fields

        private string description;

        private bool value;

        #endregion

        #region Constructors

        public BitModel()
        {
        }

        public BitModel(bool value)
        {
            this.Value = value;
        }

        public BitModel(string description, bool value) : this(value)
        {
            this.Description = description;
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value, () => this.RaisePropertyChanged(nameof(this.Description)));
        }

        public bool Value
        {
            get => this.value;
            set => this.SetProperty(ref this.value, value, () => this.RaisePropertyChanged(nameof(this.Value)));
        }

        #endregion
    }
}
