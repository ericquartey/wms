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

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Lists
{
    /// <summary>
    /// Interaction logic for ListsNavigationButtonView.xaml
    /// </summary>
    public partial class ListsNavigationButtonView : UserControl
    {
        #region Constructors

        public ListsNavigationButtonView()
        {
            this.InitializeComponent();
            this.DataContext = new ListsNavigationButtonViewModel();
        }

        #endregion Constructors
    }
}
