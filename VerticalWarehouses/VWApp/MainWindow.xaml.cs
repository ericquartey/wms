using System.Windows;
using Ferretto.VW.Utils.Source;
using Ferretto.VW.Navigation;
using System.Diagnostics;

namespace Ferretto.VW.VWApp
{
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.InitializeComponent();
            NavigationService.BackToVWAppEventHandler += this.RestoreVWAppWindow;
        }

        #endregion Constructors

        #region Properties

        public string PasswordLogin { get; set; }
        public string UserLogin { get; set; }

        #endregion Properties

        #region Methods

        public void LoginButtonMethod(object sender, RoutedEventArgs e)
        {
            this.LoginMethod();
        }

        private bool CheckLoginInput(string user, string password)
        {
            Debug.Print("VWApp::CheckLoginInput executed.\n");
            return true;
            //TODO: check correctness of inputs
        }

        private void LoginMethod()
        {
            this.UserLogin = this.UserLoginTextBox.Text;
            this.PasswordLogin = this.PasswordLoginTextBox.Text;

            if (this.CheckLoginInput(this.UserLogin, this.PasswordLogin))
            {
                if (this.UserLogin == "Installer")
                { //Instances of differents' projects MainWindow are declared in App.xaml.cs, so that there will be no running processes after closing VWApp.
                    ((App)Application.Current).InstallationAppMainWindow = new InstallationApp.MainWindow();
                    ((App)Application.Current).InstallationAppMainWindow.Show();
                    this.Hide();
                    this.UserLoginTextBox.Text = "";
                    this.PasswordLoginTextBox.Text = "";
                }
                else if (this.UserLogin == "Operator")
                {
                    ((App)Application.Current).OperatorAppMainWindow = new OperatorApp.MainWindow();
                    ((App)Application.Current).OperatorAppMainWindow.Show();
                    this.Hide();
                    this.UserLoginTextBox.Text = "";
                    this.PasswordLoginTextBox.Text = "";
                }
                else //TODO: remove this block once CheckLoginInput is implemented.
                {
                    this.UserLoginTextBox.Text = "";
                    this.PasswordLoginTextBox.Text = "";
                }
            }
            else
            {
                this.UserLoginTextBox.Text = "";
                this.PasswordLoginTextBox.Text = "";
            }
        }

        private void RestoreVWAppWindow()
        {
            this.Show();
        }

        #endregion Methods
    }
}
