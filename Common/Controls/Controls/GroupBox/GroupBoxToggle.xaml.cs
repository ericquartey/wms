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

namespace Ferretto.Common.Controls
{
    /// <summary>
    /// Interaction logic for GroupBoxToggle.xaml
    /// </summary>
    public partial class GroupBoxToggle : UserControl
    {
        public GroupBoxToggle()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(GroupBoxToggle), new FrameworkPropertyMetadata(OnTitleChanged));

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }
        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GroupBoxToggle groupBoxToggle)
            {
                var newTitle = (string)e.NewValue;
                groupBoxToggle.TitleLabel.Content = newTitle;

            }

        }
    }
}
