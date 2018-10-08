using System.Windows;
using Ferretto.VW.Utils.Source;

namespace Ferretto.VW.VWApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructors

        public MainWindow()
        {
            this.CellsManagerInstance = new CellsManager();
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public CellsManager CellsManagerInstance { get; set; }
        public string UserLogin { get; set; }

        #endregion Properties

        #region Methods

        public void LoginButtonMethod(object sender, RoutedEventArgs e)
        {
            this.LoginMethod();
        }

        private void LoginMethod()
        {
            this.UserLogin = this.LoginTextBox.Text;
            if (this.UserLogin == "Installer")
            {
                InstallationApp.MainWindow installMainWindow = new InstallationApp.MainWindow(this.CellsManagerInstance);
                installMainWindow.Show();
                this.Hide();
                this.LoginTextBox.Text = "";
            }
            else if (this.UserLogin == "Operator")
            {
                this.LoginTextBox.Text = "";
                OperatorApp.MainWindow operatorMainWindow = new OperatorApp.MainWindow();
                operatorMainWindow.Show();
                this.Hide();
            }
            else
            {
                this.LoginTextBox.Text = "";
            }
        }

        #endregion Methods
    }
}
