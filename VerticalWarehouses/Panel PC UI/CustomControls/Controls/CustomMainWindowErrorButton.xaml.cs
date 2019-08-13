using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Controls.Controls
{
    /// <summary>
    /// Interaction logic for CustomMainWindowErrorButton.xaml
    /// </summary>
    public partial class CustomMainWindowErrorButton : PpcControl
    {
        #region Fields

        public static readonly DependencyProperty ContentTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(CustomMainWindowErrorButton));

        public static readonly DependencyProperty CustomCommandProperty = DependencyProperty.Register("CustomCommand", typeof(ICommand), typeof(CustomMainWindowErrorButton));

        #endregion

        #region Constructors

        public CustomMainWindowErrorButton()
        {
            this.InitializeComponent();

            this.PresentationType = PresentationTypes.Error;
        }

        #endregion
    }
}
