using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    /// <summary>
    /// Interaction logic for HelpMainWindow.xaml
    /// </summary>
    public partial class HelpMainWindow : Window, IHelpMainWindow
    {
        #region Fields

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public HelpMainWindow(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public ContentPresenter HelpMainWindowContentRegion => this.HelpContentRegion;

        #endregion

        #region Methods

        private void Button_Click(Object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        #endregion
    }
}
