using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ferretto.Common.Controls.WPF
{
    public partial class TrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CommandDoubleClickProperty = DependencyProperty.Register(
            nameof(CommandDoubleClick), typeof(ICommand), typeof(TrayControl), new UIPropertyMetadata(OnCommandDoubleClickChanged));

        /// <summary>
        /// Lisrt of compartments
        /// </summary>
        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(
            nameof(Compartments), typeof(IEnumerable<IDrawableCompartment>), typeof(TrayControl));

        public static readonly DependencyProperty DefaultCompartmentColorProperty = DependencyProperty.Register(
                            nameof(DefaultCompartmentColor), typeof(string), typeof(TrayControl));

        public static readonly DependencyProperty DimensionHeightProperty = DependencyProperty.Register(
            nameof(DimensionHeight), typeof(double), typeof(TrayControl));

        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(
            nameof(DimensionWidth), typeof(double), typeof(TrayControl));

        public static readonly DependencyProperty GridLinesColorProperty = DependencyProperty.Register(
            nameof(GridLinesColor), typeof(Brush), typeof(TrayControl), new UIPropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(
            nameof(IsCompartmentSelectable), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            nameof(IsReadOnly), typeof(bool), typeof(TrayControl));

        public static readonly DependencyProperty OriginHorizontalProperty = DependencyProperty.Register(
            nameof(OriginHorizontal), typeof(OriginHorizontal), typeof(TrayControl), new FrameworkPropertyMetadata(OriginHorizontal.Left));

        public static readonly DependencyProperty OriginVerticalProperty = DependencyProperty.Register(
            nameof(OriginVertical), typeof(OriginVertical), typeof(TrayControl), new FrameworkPropertyMetadata(OriginVertical.Bottom));

        public static readonly DependencyProperty RulerFontSizeProperty = DependencyProperty.Register(
            nameof(RulerFontSize), typeof(int), typeof(TrayControl), new UIPropertyMetadata(8));

        public static readonly DependencyProperty RulerForegroundProperty = DependencyProperty.Register(
            nameof(RulerForeground), typeof(Brush), typeof(TrayControl), new UIPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty RulerInfoProperty = DependencyProperty.Register(
            nameof(RulerInfo), typeof(string), typeof(TrayControl));

        public static readonly DependencyProperty RulerStepProperty = DependencyProperty.Register(
            nameof(RulerStep), typeof(int), typeof(TrayControl), new UIPropertyMetadata(100));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(
            nameof(SelectedColorFilterFunc), typeof(Func<IDrawableCompartment, IDrawableCompartment, string>), typeof(TrayControl));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(IDrawableCompartment), typeof(TrayControl));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(
            nameof(ShowBackground), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(
            nameof(ShowInfo), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowLittleMarkProperty = DependencyProperty.Register(
           nameof(ShowLittleMark), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMarkProperty = DependencyProperty.Register(
          nameof(ShowMark), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMiddleMarkProperty = DependencyProperty.Register(
           nameof(ShowMiddleMark), typeof(bool), typeof(TrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(
            nameof(ShowRuler), typeof(bool), typeof(TrayControl));

        public static readonly DependencyProperty TrayWidthProperty =
            DependencyProperty.Register(nameof(TrayWidth), typeof(double), typeof(TrayControl));

        private const string DefaultCompartmentColorResourceName = "DefaultCompartmentColor";

        #endregion

        #region Constructors

        public TrayControl()
        {
            this.InitializeComponent();
            var trayControl = this;
            this.RootTrayGrid.DataContext = trayControl;
            this.SizeChanged += this.TrayControl_SizeChanged;
            this.LoadStyle();
        }

        #endregion

        #region Properties

        public ICommand CommandDoubleClick
        {
            get => (ICommand)this.GetValue(CommandDoubleClickProperty);
            set => this.SetValue(CommandDoubleClickProperty, value);
        }

        public IEnumerable<IDrawableCompartment> Compartments
        {
            get => (IEnumerable<IDrawableCompartment>)this.GetValue(CompartmentsProperty);
            set => this.SetValue(CompartmentsProperty, value);
        }

        public string DefaultCompartmentColor
        {
            get => (string)this.GetValue(DefaultCompartmentColorProperty);
            set => this.SetValue(DefaultCompartmentColorProperty, value);
        }

        public double DimensionHeight
        {
            get => (double)this.GetValue(DimensionHeightProperty);
            set => this.SetValue(DimensionHeightProperty, value);
        }

        public double DimensionWidth
        {
            get => (double)this.GetValue(DimensionWidthProperty);
            set => this.SetValue(DimensionWidthProperty, value);
        }

        public Brush GridLinesColor
        {
            get => (Brush)this.GetValue(GridLinesColorProperty);
            set => this.SetValue(GridLinesColorProperty, value);
        }

        public bool IsCompartmentSelectable
        {
            get => (bool)this.GetValue(IsCompartmentSelectableProperty);
            set => this.SetValue(IsCompartmentSelectableProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)this.GetValue(IsReadOnlyProperty);
            set => this.SetValue(IsReadOnlyProperty, value);
        }

        public OriginHorizontal OriginHorizontal
        {
            get => (OriginHorizontal)this.GetValue(OriginHorizontalProperty);
            set => this.SetValue(OriginHorizontalProperty, value);
        }

        public OriginVertical OriginVertical
        {
            get => (OriginVertical)this.GetValue(OriginVerticalProperty);
            set => this.SetValue(OriginVerticalProperty, value);
        }

        public int RulerFontSize
        {
            get => (int)this.GetValue(RulerFontSizeProperty);
            set => this.SetValue(RulerFontSizeProperty, value);
        }

        public Brush RulerForeground
        {
            get => (Brush)this.GetValue(RulerForegroundProperty);
            set => this.SetValue(RulerForegroundProperty, value);
        }

        public string RulerInfo
        {
            get => (string)this.GetValue(RulerInfoProperty);
            set => this.SetValue(RulerInfoProperty, value);
        }

        public int RulerStep
        {
            get => (int)this.GetValue(RulerStepProperty);
            set => this.SetValue(RulerStepProperty, value);
        }

        public Func<IDrawableCompartment, IDrawableCompartment, string> SelectedColorFilterFunc
        {
            get => (Func<IDrawableCompartment, IDrawableCompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public IDrawableCompartment SelectedItem
        {
            get => (IDrawableCompartment)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public bool ShowBackground
        {
            get => (bool)this.GetValue(ShowBackgroundProperty);
            set => this.SetValue(ShowBackgroundProperty, value);
        }

        public bool ShowInfo
        {
            get => (bool)this.GetValue(ShowInfoProperty);
            set => this.SetValue(ShowInfoProperty, value);
        }

        public bool ShowLittleMark
        {
            get => (bool)this.GetValue(ShowLittleMarkProperty);
            set => this.SetValue(ShowLittleMarkProperty, value);
        }

        public bool ShowMark
        {
            get => (bool)this.GetValue(ShowMarkProperty);
            set => this.SetValue(ShowMarkProperty, value);
        }

        public bool ShowMiddleMark
        {
            get => (bool)this.GetValue(ShowMiddleMarkProperty);
            set => this.SetValue(ShowMiddleMarkProperty, value);
        }

        public bool ShowRuler
        {
            get => (bool)this.GetValue(ShowRulerProperty);
            set => this.SetValue(ShowRulerProperty, value);
        }

        public double TrayWidth
        {
            get => (double)this.GetValue(TrayWidthProperty);
            set => this.SetValue(TrayWidthProperty, value);
        }

        #endregion

        #region Methods

        public static void OnCommandDoubleClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control)
            {
                if (e.NewValue != null && e.OldValue == null)
                {
                    control.MouseDoubleClick += OnMouseDoubleClick;
                }
                else if ((e.NewValue == null) && (e.OldValue != null))
                {
                    control.MouseDoubleClick -= OnMouseDoubleClick;
                }
            }
        }

        private static void OnMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            var command = (ICommand)control.GetValue(CommandDoubleClickProperty);
            var commandParameter = control.GetValue(CommandDoubleClickProperty);

            if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        private void LoadStyle()
        {
            var dictionary = new ResourceDictionary();
            var resourceUri = $"/{this.GetType().Namespace};component/Styles/{nameof(TrayControl)}.xaml";
            dictionary.Source = new Uri(resourceUri, UriKind.Relative);
            this.Resources.MergedDictionaries.Add(dictionary);
            this.DefaultCompartmentColor = this.Resources[DefaultCompartmentColorResourceName].ToString();
        }

        private void TrayControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.CanvasListBoxControl.SetSize(e.NewSize.Height - 1, e.NewSize.Width - 1);
            this.TrayWidth = this.CanvasListBoxControl.TrayWidth;
        }

        #endregion
    }
}
