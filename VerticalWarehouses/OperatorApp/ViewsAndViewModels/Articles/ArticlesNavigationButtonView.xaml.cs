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

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Articles
{
    /// <summary>
    /// Interaction logic for ArticlesNavigationButtonView.xaml
    /// </summary>
    public partial class ArticlesNavigationButtonView : UserControl
    {
        #region Constructors

        public ArticlesNavigationButtonView()
        {
            this.InitializeComponent();
            this.DataContext = new ArticlesNavigationButtonViewModel();
        }

        #endregion Constructors
    }
}
