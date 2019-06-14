using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Ferretto.Common.BLL.Interfaces;

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
