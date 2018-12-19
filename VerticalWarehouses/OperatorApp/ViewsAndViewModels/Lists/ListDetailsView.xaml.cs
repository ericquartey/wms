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
    /// Interaction logic for ListDetailsView.xaml
    /// </summary>
    public partial class ListDetailsView : UserControl
    {
        #region Constructors

        public ListDetailsView()
        {
            this.InitializeComponent();
            this.DataContext = new ListDetailsViewModel();
        }

        #endregion Constructors
    }
}
