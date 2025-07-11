﻿using Prism.Mvvm;

namespace Ferretto.VW.Simulator.Services.Models
{
    public class BitModel : BindableBase
    {
        #region Fields

        private string description;

        private string name;

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

        public BitModel(string name, bool value, string description = "") : this(value)
        {
            this.Name = name;
            this.Description = description;
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        public bool Value
        {
            get => this.value;
            set => this.SetProperty(ref this.value, value);
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.Value.ToString();
        }

        #endregion
    }
}
