using System.Windows;
using Ferretto.VW.Navigation;
using System.Diagnostics;
using System;

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

        public void CloseSystemButtonMethod(object sender, RoutedEventArgs e)
        {
            this.CloseSystem();
        }

        public void LoginButtonMethod(object sender, RoutedEventArgs e)
        {
            this.LoginMethod();
        }

        public void OtherActionButtonMethod(object sender, RoutedEventArgs e)
        {
            this.OtherAction();
        }

        private bool CheckLoginInput(string user, string password)
        {
            Debug.Print("VWApp::CheckLoginInput executed.\n");
            return true;
            //TODO: check correctness of inputs
        }

        private void CloseSystem()
        {
            //TODO: implement system closing
            Debug.Print("VWApp::CloseSystem executed.\n");
        }

        private void LoginMethod()
        {
            this.UserLogin = this.UserLoginTextBox.Text;
            this.PasswordLogin = this.PasswordLoginTextBox.Text;

            if (this.CheckLoginInput(this.UserLogin, this.PasswordLogin))
            {
                if (this.UserLogin == "Installer")
                { //Instances of differents' projects MainWindow are declared in App.xaml.cs, so that there will be no running processes after closing VWApp.
                    ((App)Application.Current).InstallationMainWindowInstance = new InstallationApp.MainWindow();
                    ((App)Application.Current).InstallationMainWindowInstance.Show();
                    this.Hide();
                    this.UserLoginTextBox.Text = "";
                    this.PasswordLoginTextBox.Text = "";
                }
                else if (this.UserLogin == "Operator")
                {
                    ((App)Application.Current).OperatorMainWindowInstance = new OperatorApp.MainWindow();
                    ((App)Application.Current).OperatorMainWindowInstance.Show();
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
                //TODO: open a popup to communicate to the user that the login info are not correct
                this.UserLoginTextBox.Text = "";
                this.PasswordLoginTextBox.Text = "";
            }
        }

        private void OtherAction()
        {
            //TODO: implement system closing
            Debug.Print("VWApp::OtherAction executed.\n");
        }

        private void RestoreVWAppWindow()
        {
            this.Show();
        }

        #endregion Methods
    }
}
