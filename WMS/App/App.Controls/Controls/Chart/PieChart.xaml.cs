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
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart : UserControl
    {
        #region Fields

        public static readonly DependencyProperty MapModelProperty = DependencyProperty.Register(
           nameof(MapModel), typeof(bool), typeof(PieChart), new FrameworkPropertyMetadata(null));

        #endregion

        #region Constructors

        public PieChart()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public IList<IMapModel> MapModel { get; set; }

        #endregion
    }
}
