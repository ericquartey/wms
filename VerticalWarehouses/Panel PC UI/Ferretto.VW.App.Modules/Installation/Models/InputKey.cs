using System;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Installation.Models
{
    public class InputKey : BindableBase
    {
        #region Fields

        private bool isHighlighted;

        #endregion

        #region Constructors

        public InputKey(string key, string displayKey = null)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.Key = key;
            this.DisplayKey =
                displayKey
                ??
                key.Replace("\n", "<LF>").Replace("\r", "<CR>").Replace("\t", "<TAB>");
        }

        #endregion

        #region Properties

        public string DisplayKey { get; }

        public bool IsHighlighted
        {
            get => this.isHighlighted;
            set => this.SetProperty(ref this.isHighlighted, value);
        }

        public string Key { get; }

        #endregion
    }
}
