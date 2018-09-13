using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Ferretto.Common.Controls
{
    public partial class ComboBox : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(ComboBox), new PropertyMetadata(default(string)));

        public string Label
        {
            get => (string) this.GetValue(LabelProperty);
            set => this.SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty AllowNullInputProperty = DependencyProperty.Register(
            nameof(AllowNullInput), typeof(bool), typeof(ComboBox), new PropertyMetadata(default(bool)));

        public bool AllowNullInput
        {
            get => (bool) this.GetValue(AllowNullInputProperty);
            set => this.SetValue(AllowNullInputProperty, value);
        }

        public static readonly DependencyProperty AutoCompleteProperty = DependencyProperty.Register(
            nameof(AutoComplete), typeof(bool), typeof(ComboBox), new PropertyMetadata(default(bool)));

        public bool AutoComplete
        {
            get => (bool) this.GetValue(AutoCompleteProperty);
            set => this.SetValue(AutoCompleteProperty, value);
        }

        public static readonly DependencyProperty DisplayMemberProperty = DependencyProperty.Register(
            nameof(DisplayMember), typeof(string), typeof(ComboBox), new PropertyMetadata("Description"));

        public string DisplayMember
        {
            get => (string) this.GetValue(DisplayMemberProperty);
            set => this.SetValue(DisplayMemberProperty, value);
        }

        public static readonly DependencyProperty ValueMemberProperty = DependencyProperty.Register(
            nameof(ValueMember), typeof(string), typeof(ComboBox), new PropertyMetadata("Value"));

        public string ValueMember
        {
            get => (string) this.GetValue(ValueMemberProperty);
            set => this.SetValue(ValueMemberProperty, value);
        }

        public static readonly DependencyProperty IsTextEditableProperty = DependencyProperty.Register(
            nameof(IsTextEditable), typeof(bool), typeof(ComboBox), new PropertyMetadata(false));

        public bool IsTextEditable
        {
            get => (bool) this.GetValue(IsTextEditableProperty);
            set => this.SetValue(IsTextEditableProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ComboBox),
            new PropertyMetadata(default(IEnumerable)));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable) this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(ComboBox),
            new PropertyMetadata(default(object)));

        public object SelectedValue
        {
            get => this.GetValue(SelectedValueProperty);
            set => this.SetValue(SelectedValueProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(ComboBox),
                new UIPropertyMetadata());

        public object SelectedItem
        {
            get => (object) this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedTextProperty = DependencyProperty.Register(
            nameof(SelectedText), typeof(string), typeof(ComboBox), new PropertyMetadata(default(string)));

        public string SelectedText
        {
            get => (string) this.GetValue(SelectedTextProperty);
            set => this.SetValue(SelectedTextProperty, value);
        }

        #endregion

        #region Properties

        public string Text => this.dxeComboBoxEdit.SelectedText;

        #endregion

        #region Ctor

        public ComboBox()
        {
            this.InitializeComponent();
            this.gridComboBox.DataContext = this;
        }

        #endregion
    }
}
