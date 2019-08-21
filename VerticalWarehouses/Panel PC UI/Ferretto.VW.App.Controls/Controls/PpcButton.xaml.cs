using System.Windows;
using System.Windows.Controls;

namespace Ferretto.VW.App.Controls.Controls
{
    public partial class PpcButton : Button
    {
        #region Fields

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive),
            typeof(bool),
            typeof(PpcButton));

        #endregion

        #region Constructors

        public PpcButton()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public bool IsActive
        {
            get => (bool)this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        #endregion
    }
}
