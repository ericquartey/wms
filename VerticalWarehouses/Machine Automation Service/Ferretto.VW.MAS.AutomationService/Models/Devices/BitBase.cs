using System;
using Prism.Mvvm;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class BitBase
    {
        #region Fields

        private string description;

        private bool isReadable;

        private string name;

        private bool value;

        #endregion

        #region Constructors

        public BitBase()
        {
        }

        public BitBase(bool? value)
        {
            if (!(value is null))
            {
                this.IsReadable = true;
                this.Value = (bool)value;
            }
        }

        public BitBase(string name, bool? value, string description = "") : this(value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            this.Description = description;
            this.Name = name.Substring(0, Math.Min(name.Length, 3));
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.description = value;
        }

        public bool IsReadable
        {
            get => this.isReadable;
            set => this.isReadable = value;
        }

        public string Name
        {
            get => this.name;
            set => this.name = value;
        }

        public bool Value
        {
            get => this.value;
            set => this.value = value;
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
