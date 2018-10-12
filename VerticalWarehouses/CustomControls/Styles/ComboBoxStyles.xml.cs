using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ferretto.VW.CustomControls.Styles
{
    public partial class ComboBoxStyles : ResourceDictionary
    {
        #region Constructors

        public ComboBoxStyles()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        public void OpenDropDownList(object sender, MouseButtonEventArgs e)
        {
            Debug.Print("flag\n");
            if (sender is ComboBox comboBox)
            {
                comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
            }
        }

        #endregion Methods
    }
}
