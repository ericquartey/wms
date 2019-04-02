using System.Windows;
using DevExpress.Xpf.Bars;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.Controls
{
    public partial class ActionBarItem : BarButtonItem
    {
        #region Fields

        public static readonly DependencyProperty ReasonProperty = DependencyProperty.Register(
            nameof(Reason),
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

        public string Reason
        {
            get => this.Policy?.Reason;
            set => this.SetValue(ReasonProperty, value);
        }

        #endregion
    }
}
