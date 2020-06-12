using System.Windows;
using System.Windows.Input;

namespace Ferretto.VW.Installer.Controls
{
    public partial class PpcMessageBox
    {
        #region Fields

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(PpcMessageBox));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(PpcMessageBox));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
               nameof(Title),
               typeof(string),
               typeof(PpcMessageBox));

        #endregion

        #region Constructors

        public PpcMessageBox()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        public string Message
        {
            get => (string)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        #endregion
    }
}
