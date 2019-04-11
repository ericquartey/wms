using System.Windows;
using DevExpress.Xpf.Bars;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Controls
{
    public partial class ActionBarItem : BarButtonItem
    {
        #region Fields

        public static readonly DependencyProperty TooltipTextProperty = DependencyProperty.Register(
            nameof(TooltipText),
            typeof(string),
            typeof(ActionBarItem),
            new UIPropertyMetadata(null));

        #endregion

        #region Constructors

        public ActionBarItem()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Properties

        public string TooltipText
        {
            get => (string)this.GetValue(TooltipTextProperty);
            set => this.SetValue(TooltipTextProperty, value);
        }

        #endregion
    }
}
