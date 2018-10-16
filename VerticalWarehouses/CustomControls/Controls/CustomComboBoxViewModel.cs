using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Ferretto.VW.CustomControls.Controls
{
    internal class CustomComboBoxViewModel : BindableBase
    {
        #region Fields

        private bool isItemsPopUpOpen;

        #endregion Fields

        #region Properties

        public Boolean IsItemsPopUpOpen { get => this.isItemsPopUpOpen; set => this.SetProperty(ref this.isItemsPopUpOpen, value); }
        public List<String> Items { get; set; }

        #endregion Properties
    }
}
