using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public partial class WmsTrayControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty CanvasMinHeightProperty = DependencyProperty.Register(nameof(CanvasMinHeight), typeof(double), typeof(WmsTrayControl), new UIPropertyMetadata(150.0));

        public static readonly DependencyProperty CompartmentsProperty = DependencyProperty.Register(nameof(Compartments), typeof(IEnumerable<ICompartment>), typeof(WmsTrayControl));

        public static readonly DependencyProperty DimensionHeightProperty = DependencyProperty.Register(nameof(DimensionHeight), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty DimensionWidthProperty = DependencyProperty.Register(nameof(DimensionWidth), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty GridLinesColorProperty = DependencyProperty.Register(nameof(GridLinesColor), typeof(Brush), typeof(WmsTrayControl), new UIPropertyMetadata(Brushes.LightGray));

        public static readonly DependencyProperty IsCompartmentSelectableProperty = DependencyProperty.Register(nameof(IsCompartmentSelectable), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(WmsTrayControl));

        public static readonly DependencyProperty OriginXProperty = DependencyProperty.Register(nameof(OriginX), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty OriginYProperty = DependencyProperty.Register(nameof(OriginY), typeof(double), typeof(WmsTrayControl));

        public static readonly DependencyProperty RulerFontSizeProperty = DependencyProperty.Register(nameof(RulerFontSize), typeof(int), typeof(WmsTrayControl), new UIPropertyMetadata(8));

        public static readonly DependencyProperty RulerForegroundProperty = DependencyProperty.Register(nameof(RulerForeground), typeof(Brush), typeof(WmsTrayControl), new UIPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty RulerInfoProperty = DependencyProperty.Register(nameof(RulerInfo), typeof(string), typeof(WmsTrayControl));

        public static readonly DependencyProperty RulerStepProperty = DependencyProperty.Register(nameof(RulerStep), typeof(int), typeof(WmsTrayControl), new UIPropertyMetadata(100));

        public static readonly DependencyProperty SelectedColorFilterFuncProperty = DependencyProperty.Register(nameof(SelectedColorFilterFunc), typeof(Func<ICompartment, ICompartment, string>), typeof(WmsTrayControl));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(ICompartment), typeof(WmsTrayControl));

        public static readonly DependencyProperty ShowBackgroundProperty = DependencyProperty.Register(nameof(ShowBackground), typeof(bool), typeof(WmsTrayControl));

        public static readonly DependencyProperty ShowInfoProperty = DependencyProperty.Register(nameof(ShowInfo), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowLittleMarkProperty = DependencyProperty.Register(nameof(ShowLittleMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMarkProperty = DependencyProperty.Register(nameof(ShowMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowMiddleMarkProperty = DependencyProperty.Register(nameof(ShowMiddleMark), typeof(bool), typeof(WmsTrayControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty ShowRulerProperty = DependencyProperty.Register(nameof(ShowRuler), typeof(bool), typeof(WmsTrayControl));

        #endregion Fields

        #region Constructors

        public WmsTrayControl()
        {
            this.InitializeComponent();
            this.RootTrayGrid.DataContext = this;
            this.SizeChanged += this.WmsTrayControl_SizeChanged;
            this.Unloaded += this.WmsTrayControl_Unloaded;
        }

        #endregion Constructors

        #region Properties

        public double CanvasMinHeight
        {
            get => (double)this.GetValue(CanvasMinHeightProperty);
            set => this.SetValue(CanvasMinHeightProperty, value);
        }

        public IEnumerable<ICompartment> Compartments
        {
            get => (IEnumerable<ICompartment>)this.GetValue(CompartmentsProperty);
            set => this.SetValue(CompartmentsProperty, value);
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

        public double OriginX
        {
            get => (double)this.GetValue(OriginXProperty);
            set => this.SetValue(OriginXProperty, value);
        }

        public double OriginY
        {
            get => (double)this.GetValue(OriginYProperty);
            set => this.SetValue(OriginYProperty, value);
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

        public Func<ICompartment, ICompartment, string> SelectedColorFilterFunc
        {
            get => (Func<ICompartment, ICompartment, string>)this.GetValue(
                SelectedColorFilterFuncProperty);
            set => this.SetValue(SelectedColorFilterFuncProperty, value);
        }

        public ICompartment SelectedItem
        {
            get => (ICompartment)this.GetValue(SelectedItemProperty);
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

        #endregion Properties

        #region Methods

        private void WmsTrayControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.OriginX == 0 && this.OriginY == 0)
            {
                this.OriginY = this.DimensionHeight;
            }
            this.CanvasListBoxControl.SetSize(e.NewSize.Height, e.NewSize.Width);
        }

        private void WmsTrayControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged -= this.WmsTrayControl_SizeChanged;
        }

        #endregion Methods
    }
}
