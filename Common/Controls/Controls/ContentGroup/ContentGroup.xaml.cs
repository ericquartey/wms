using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class ContentGroup : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ControlContentProperty =
            DependencyProperty.Register(nameof(ControlContent), typeof(object), typeof(ContentGroup));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(ContentGroup));

        #endregion

        #region Constructors

        public ContentGroup()
        {
            this.InitializeComponent();
            this.GridContentGroup.DataContext = this;
        }

        #endregion

        #region Properties

        public object ControlContent
        {
            get => this.GetValue(ControlContentProperty);
            set => this.SetValue(ControlContentProperty, value);
        }

        public string Label
        {
            get => (string)this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        #endregion
    }
}
