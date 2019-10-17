using System;

namespace Ferretto.VW.MAS.AutomationService.Models
{
    public class BitInfo
    {
        #region Fields

        private string description;

        private bool isUsed;

        private string name;

        private bool value;

        #endregion

        #region Constructors

        public BitInfo()
        {
        }

        public BitInfo(bool? value)
        {
            if (!(value is null))
            {
                this.IsUsed = true;
                this.Value = (bool)value;
            }
        }

        public BitInfo(string name, bool? value, string description = "")
            : this(value)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            this.Description = description;
            this.Name = name.Substring(0, Math.Min(name.Length, 4));
        }

        #endregion

        #region Properties

        public string Description
        {
            get => this.description;
            set => this.description = value;
        }

        public bool IsUsed
        {
            get => this.isUsed;
            set => this.isUsed = value;
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
